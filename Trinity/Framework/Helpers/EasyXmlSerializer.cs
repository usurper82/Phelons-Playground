using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Trinity.Framework.Helpers
{
    using System;

    public class EasyXmlSerializer
    {
        public static string Serialize<T>(T value)
        {
            var emptyNamepsaces = new XmlSerializerNamespaces(new[]
            {
                XmlQualifiedName.Empty
            });
            var serializer = new XmlSerializer(value.GetType());
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };
            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, value, emptyNamepsaces);
                return stream.ToString();
            }
        }

        public static T Deserialize<T>(string input)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(input))
            {
                return (T)serializer.Deserialize(reader);
            }
        }    /// <summary>
             /// Serializes an object.
             /// </summary>
             /// <typeparam name="T"></typeparam>
             /// <param name="serializableObject"></param>
             /// <param name="fileName"></param>
        public void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }
        }


        /// <summary>
        /// Deserializes an xml file into an object list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public T DeSerializeObject<T>(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                        reader.Close();
                    }

                    read.Close();
                }
            }
            catch (Exception ex)
            {
                //Log exception here
            }

            return objectOut;
        }
    }
}