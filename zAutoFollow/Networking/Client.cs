using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using AutoFollow.Events;
using AutoFollow.Resources;

namespace AutoFollow.Networking
{
    public class Client
    {
        public delegate void ClientMessageDelegate(Message message);

        private static ChannelFactory<IService> _httpFactory;
        private static IService _httpProxy;

        public static bool FirstConnectionAttempt = true;
        public static DateTime LastUnexpectedException = DateTime.MinValue;
        public static DateTime LastExpectedException = DateTime.MinValue;
        public static DateTime LastClientUpdate = DateTime.MinValue;
        public static DateTime EarliestNextAttempt = DateTime.MinValue;
        public static DateTime LastSummaryTime = DateTime.MinValue;

        public static bool ClientConnected { get; private set; }
        public static int ConnectionAttempts { get; set; }
        public static int ExpectedExceptionCount { get; set; }
        public static bool IsValid => ClientConnected && _httpProxy != null;

        public static event Server.ServiceDelegate OnClientInitialized;
        public static event ClientMessageDelegate OnClientUpdated;

        public static void ClientInitialize()
        {
            if (!AutoFollow.Enabled)
                return;

            if (DateTime.UtcNow.Subtract(LastUnexpectedException).TotalSeconds < 1)
                return;

            ConnectionAttempts++;

            try
            {
                if (!ClientConnected && AutoFollow.Enabled)
                {
                    var serverPort = Settings.Network.ServerPort;
                    Server.ServerUri = new Uri(Server.ServerUri.AbsoluteUri.Replace(Server._basePort.ToString(), serverPort.ToString()));

                    Log.Info("Initializing Client Service connection to {0} Attempt={1}", Server.ServerUri.AbsoluteUri + "Follow", ConnectionAttempts);

                    var binding = new BasicHttpBinding
                    {
                        OpenTimeout = TimeSpan.FromMilliseconds(5000),
                        SendTimeout = TimeSpan.FromMilliseconds(5000),
                        CloseTimeout = TimeSpan.FromMilliseconds(500),
                        MaxBufferSize = int.MaxValue,
                        MaxReceivedMessageSize = int.MaxValue,
                        MaxBufferPoolSize = int.MaxValue,
                        TransferMode = TransferMode.Streamed
                    };

                    binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                    var endPointAddress = new EndpointAddress(Server.ServerUri.AbsoluteUri + "Follow/?wsdl");
                    _httpFactory = new ChannelFactory<IService>(binding, endPointAddress);
                    _httpProxy = _httpFactory.CreateChannel();

                    OnClientInitialized?.Invoke();
                    ClientConnected = true;
                }
            }
            catch (Exception ex)
            {
                Log.Info("Exception in ClientInitialize() {0} Attempt={1}", ex, ConnectionAttempts);
                LastUnexpectedException = DateTime.UtcNow;
            }

            if (ConnectionAttempts > 5 && !ClientConnected)
            {
                Thread.Sleep(5000);
            }
        }

        public static void ShutdownClient()
        {
            try
            {
                if (_httpFactory != null && (_httpFactory.State != CommunicationState.Closed || _httpFactory.State != CommunicationState.Closing))
                {
                    _httpFactory.Close();
                }
                _httpProxy = null;
            }
            catch (Exception)
            {
                _httpFactory = null;
                _httpProxy = null;
            }
            ClientConnected = false;
        }

        /// <summary>
        /// Called by Client, Receives Server Message and Sends Client Message
        /// </summary>
        internal static void ClientUpdate()
        {
            if (!AutoFollow.Enabled)
                return;

            if (Server.ServiceHost != null && Server.ServiceHost.State != CommunicationState.Closed)
            {
                Server.ShutdownServer();
            }

            if (!IsValid)
            {
                Log.Info("Client is not valid");

                if (_httpProxy != null)
                {
                    Log.Info("Shutting down Client");
                    ShutdownClient();
                }
                else
                {
                    Log.Info("Initializing Client");
                    ClientInitialize();
                    Thread.Sleep(250);
                }

                if (!IsValid || _httpProxy == null || _httpFactory.State != CommunicationState.Closed)
                {
                    Log.Info("Waiting for client connection to be valid");
                    return;
                }
            }

            try
            {
                var elapsed = DateTime.UtcNow.Subtract(LastClientUpdate).TotalMilliseconds;
                if (elapsed < 25 || elapsed < Settings.Network.UpdateInterval)
                    return;

                if (DateTime.UtcNow.Subtract(LastUnexpectedException).TotalSeconds < 2)
                    return;

                if (DateTime.UtcNow.Subtract(LastExpectedException).TotalSeconds < 1)
                    return;

                if (DateTime.UtcNow < EarliestNextAttempt)
                    return;

                if (_httpProxy == null)
                    return;

                _httpProxy.SendMessageToServer(new MessageWrapper
                {
                    PrimaryMessage = Player.CurrentMessage
                });

                var messageWrapper = _httpProxy.GetMessageFromServer();

                if (messageWrapper.PrimaryMessage == null)
                {
                    Log.Debug("Unable to get a message from Server", AutoFollow.NumberOfConnectedBots);
                    return;
                }

                UpdateDataAsClient(messageWrapper);

                Log.Debug("Communicating with {0} other bots:", AutoFollow.NumberOfConnectedBots);

                if (DateTime.UtcNow.Subtract(LastSummaryTime).TotalSeconds > 2)
                {
                    Log.Debug("{0}", messageWrapper.PrimaryMessage.ShortSummary);

                    foreach (var otherMessage in messageWrapper.OtherMessages)
                    {
                        Log.Debug("{0}", otherMessage.ShortSummary);
                    }
                    LastSummaryTime = DateTime.UtcNow;
                }

                foreach (var e in messageWrapper.PrimaryMessage.Events)
                {
                    if (!EventManager.HasFired(e))
                        Log.Verbose("Received new unfired event {0} from {1}", e.Type, e.OwnerHeroAlias);
                }

                OnClientUpdated?.Invoke(AutoFollow.ServerMessage);

                ConnectionAttempts = 0;
                ExpectedExceptionCount = 0;
            }
            catch (EndpointNotFoundException ex)
            {
                if (!FirstConnectionAttempt)
                {
                    Log.Info("Lost Connection to server...");
                    Log.Debug("EndpointNotFoundException: Could not get an update from the leader using {0}. Is the leader running? ({1})", _httpFactory.Endpoint.Address.Uri.AbsoluteUri, ex.Message);
                }

                EarliestNextAttempt = DateTime.UtcNow.Add(TimeSpan.FromSeconds(5));

                ClientConnected = false;

                if (!Service.ForceConnectionMode)
                {
                    Service.ConnectionMode = ConnectionMode.Server;
                }
            }
            catch (CommunicationException ex)
            {
                Log.Error("CommunicationException. Failed to Communicate with Server on {0} Message={1}", _httpFactory.Endpoint.Address.Uri.AbsoluteUri, ex.Message, ex.InnerException);
                LastExpectedException = DateTime.UtcNow;
                ExpectedExceptionCount++;
            }
            catch (TimeoutException ex)
            {
                Log.Debug("CommunicationException. Server failed to respond (Thread: {0})", Thread.CurrentThread.ManagedThreadId);
                LastExpectedException = DateTime.UtcNow;
                ExpectedExceptionCount++;
            }
            catch (Exception ex)
            {
                Log.Info("Exception: Could not get an update from the leader using {0}. Is the leader running?", _httpFactory.Endpoint.Address.Uri.AbsoluteUri);
                ClientConnected = false;
                LastUnexpectedException = DateTime.UtcNow;
                Log.Info(ex.ToString());
                ConnectionAttempts++;
                EarliestNextAttempt = DateTime.UtcNow.Add(TimeSpan.FromSeconds(5));
            }

            FirstConnectionAttempt = false;
        }

        private static void UpdateDataAsClient(MessageWrapper messageWrapper)
        {
            AutoFollow.CurrentParty = new List<Message>(messageWrapper.OtherMessages) {messageWrapper.PrimaryMessage};
            AutoFollow.CurrentFollowers = AutoFollow.CurrentParty.Where(o => o.IsFollower).ToList();
            var leader = AutoFollow.CurrentParty.FirstOrDefault(o => o.IsLeader);
            AutoFollow.CurrentLeader = leader ?? new Message();
            AutoFollow.NumberOfConnectedBots = Service.GetSmoothedConnectedBotCount(AutoFollow.CurrentParty);
            AutoFollow.ServerMessage = messageWrapper.PrimaryMessage;
            EventManager.Add(AutoFollow.ServerMessage.Events);
            LastClientUpdate = DateTime.UtcNow;
        }
    }
}