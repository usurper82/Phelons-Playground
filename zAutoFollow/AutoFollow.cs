﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using AutoFollow.Behaviors;
using AutoFollow.Behaviors.Structures;
using AutoFollow.Coroutines;
using AutoFollow.Events;
using AutoFollow.Networking;
using AutoFollow.ProfileTags;
using AutoFollow.Resources;
using AutoFollow.UI;
using JetBrains.Annotations;
using Trinity.Components.Combat;
using Trinity.Framework;
using Zeta.Bot;
using Zeta.Bot.Logic;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using EventManager = AutoFollow.Events.EventManager;

namespace AutoFollow
{
    public class AutoFollow : IPlugin
    {
        public static AutoFollow Instance { get; set; }
        public static InterfaceLoader<IBehavior> Behaviors;
        public static Version PluginVersion = new Version(1, 260, 1);
        internal static bool Enabled;
        internal static Message ServerMessage = new Message();
        internal static Dictionary<int, Message> ClientMessages = new Dictionary<int, Message>();
        internal static IBehavior LeaderBehavior = new Leader();
        internal static IBehavior FollowerBehavior = new FollowerCombat();
        internal static IBehavior DefaultBehavior = new BaseBehavior();
        public static List<Message> CurrentParty = new List<Message>();
        public static List<Message> CurrentFollowers = new List<Message>();
        public static Message CurrentLeader = new Message();
        public static int NumberOfConnectedBots;
        private static DateTime _lastSelectedBehavior = DateTime.MinValue;
        private static IBehavior _currentBehavior;

        #region IPlugin Members

        public Version Version => PluginVersion;

        public string Author => "xzjv";

        public string Description => "Co-op made better";

        public Window DisplayWindow => UILoader.GetSettingsWindow();

        public string Name => "AutoFollow";

        public void OnEnabled() => Enable();

        public void OnDisabled() => Disable();

        public void OnPulse() => Pulse();

        public void OnInitialize() => Initialize();

        public void OnShutdown() { }

        public bool Equals(IPlugin other) => (other.Name == Name) && (other.Version == Version);

        #endregion

        public AutoFollow()
        {
            Behaviors = new InterfaceLoader<IBehavior>();
            Instance = this;

            if (Settings.Misc.AlwaysEnablePlugin)
            {
                PluginManager.OnPluginsReloaded += PluginManager_OnPluginsReloaded;
            }
        }

        private void PluginManager_OnPluginsReloaded(object sender, EventArgs e)
        {
            foreach (var plugin in PluginManager.Plugins)
            {
                if (plugin.Plugin == this && !plugin.Enabled)
                {
                    plugin.Enabled = true;
                }
            }
        }

        public static IBehavior CurrentBehavior
        {
            get { return _currentBehavior; }
            set
            {
                if (value == null || _currentBehavior == value)
                    return;

                if (_currentBehavior != null)
                {
                    _currentBehavior.Deactivate();
                    Log.Warn("Changing behavior type from {0} to {1}", _currentBehavior.Name, value.Name);

                    // Important: need to restart profile or behavior hooks will still run even after being removed.
                    ProfileManager.Load(ProfileManager.CurrentProfile.Path);

                    Targetting.State = CombatState.Enabled;
                }

                _currentBehavior = value;
                _currentBehavior.Activate();
            }
        }

        /// <summary>
        /// Called by service communication thread.
        /// </summary>
        public void ServiceOnUpdatePreview()
        {

        }

        public static void Pulse()
        {
            if (!Enabled)
                return;

            if (ZetaDia.Globals.IsLoadingWorld)
                return;

            if (!Service.IsConnected)
            {
                Service.Connect();
                CommunicationThread.ThreadStart();
            }

            if (ZetaDia.IsInGame)
            {
                InGamePulse();
            }
            else
            {
                OutOfGamePulse();
            }

            GameUI.SafeCheckClickButtons();
        }

        internal static void OutOfGamePulse()
        {
            ChangeMonitor.CheckForChanges();
            Player.UpdateOutOfGame();
            EventManager.Update();
            SelectBehavior();
        }

        internal static void InGamePulse()
        {
            ChangeMonitor.CheckForChanges();
            Player.Update();
            SelectBehavior();
        }

        /// <summary>
        /// Select the correct behavior.
        /// </summary>
        internal static void SelectBehavior()
        {
            if (!Service.IsConnected)
                return;

            if (DateTime.UtcNow.Subtract(_lastSelectedBehavior).TotalSeconds < 5)
                return;

            _lastSelectedBehavior = DateTime.UtcNow;

            if (ProfileManager.CurrentProfile == null)
            {
                CurrentBehavior = DefaultBehavior;
                return;
            }

            // Assign from tag that has been reached in profile.
            if (AutoFollowTag.Current != null && AutoFollowTag.Current.CurrentBehavior != null)
            {
                if (CurrentBehavior == AutoFollowTag.Current.CurrentBehavior)
                    return;

                CurrentBehavior = AutoFollowTag.Current.CurrentBehavior;
                return;
            }

            // Assign from tag within profile before tag has been reached.
            var profileTag = ProfileUtils.GetProfileTag("AutoFollow");
            if (profileTag != null)
            {
                var behaviorAttr = profileTag.Attribute("behavior");
                if (behaviorAttr != null && !string.IsNullOrEmpty(behaviorAttr.Value))
                {
                    AssignBehaviorByName(behaviorAttr.Value);
                    return;
                }

                CurrentBehavior = DefaultBehavior;
                return;
            }

            CurrentBehavior = LeaderBehavior;
        }

        /// <summary>
        /// Find a leader amongst the connected bots.
        /// Server is responsible for designating a leader, other bots will do as they are told, maybe.
        /// </summary>
        public static Message SelectLeader()
        {
            if (NumberOfConnectedBots <= 0)
                return ServerMessage;

            var leader = CurrentParty.FirstOrDefault(m => m.BehaviorCategory == BehaviorCategory.Leader);
            if (leader == null)
            {
                Log.Warn("Waiting for a leader...");
                return new Message();
            }

            if (CurrentLeader.OwnerId != leader.OwnerId)
            {
                Log.Warn("Selected new leader as {0} ({1})", leader.HeroAlias, leader.OwnerId);
            }

            return leader;
        }

        public static void AssignBehaviorByName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                IBehavior behavior;
                if (Behaviors.Items.TryGetValue(name, out behavior) && CurrentBehavior != behavior)
                {
                    Log.Info("Loading behavior: {0}", name);
                    CurrentBehavior = behavior;
                }
            }
            else
            {
                Log.Info("Requested behavior '{0}' was not found", name);
                CurrentBehavior = DefaultBehavior;
                BotMain.Stop();
            }
        }

        public static void DeselectPlugin()
        {
            Enabled = false;
            var thisPlugin = PluginManager.Plugins.FirstOrDefault(p => p.Plugin.Name == "AutoFollow");
            if (thisPlugin != null)
                thisPlugin.Enabled = false;                    
        }

        private void Disable()
        {
            Enabled = false;
            TrinityCombat.Party = DefaultProviders.Party;
            Log.Info("Plugin disabled! ");
            CurrentBehavior?.Deactivate();
            BotMain.OnStart -= BotMain_OnStart;
            BotMain.OnStop -= BotMain_OnStop;
            EventManager.Disable();
            EventManager.OnPulseOutOfGame -= Pulse;
            Service.OnUpdatePreview -= ServiceOnUpdatePreview;
            Server.ShutdownServer();
            Client.ShutdownClient();         
            TabUi.RemoveTab();
            ChangeMonitor.Disable();
        }

        private void Enable()
        {
            if (!Application.Current.CheckAccess()) return;
            Enabled = true;
            TrinityCombat.Party = new AutoFollowPartyProvider();
            Log.Info(" v{0} Enabled", Version);
            BotMain.OnStart += BotMain_OnStart;
            BotMain.OnStop += BotMain_OnStop;
            CurrentBehavior = DefaultBehavior;
            EventManager.Enable();
            TabUi.InstallTab();
            ChangeMonitor.Enable();
            Server.ServerStartAttempts = 0;
            Client.ConnectionAttempts = 0;
            Service.Connect();
            CommunicationThread.ThreadStart();
            TreeHooks.Instance.OnHooksCleared += OnHooksCleared;
            
        }

        private void OnHooksCleared(object sender, EventArgs e)
        {
            CurrentBehavior.Activate();
        }

        private void BotMain_OnStart(IBot bot)
        {
            if (!Service.IsConnected)
            {
                Service.Connect();
                CommunicationThread.ThreadStart();
            }

            SelectBehavior();
            CurrentBehavior.Activate();
        }

        private void BotMain_OnStop(IBot bot)
        {
            if (Service.IsConnected)
            {
                Service.Disconnect();
                CommunicationThread.ThreadShutdown();
                CurrentParty.Clear();
                ServerMessage = null;
                ClientMessages.Clear();                
            }
            CurrentBehavior.Deactivate();
        }

        private static void Initialize()
        {
            if (!Application.Current.CheckAccess()) return;

            Conditions.Initialize();
            Service.Initialize();
        }

        public static Message GetUpdatedMessage(Message message)
        {
            return CurrentParty.FirstOrDefault(p => p.HeroId == message.HeroId);
        }

        public static Vector3 GetUpdatedPosition(Message message)
        {
            var partyMember = GetUpdatedMessage(message);
            return partyMember?.Position ?? Vector3.Zero;
        }





    }
}


