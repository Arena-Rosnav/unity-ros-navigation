//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.Gazebo
{
    [Serializable]
    public class DeleteModelRequest : Message
    {
        public const string k_RosMessageName = "gazebo_msgs/DeleteModel";
        public override string RosMessageName => k_RosMessageName;

        public string model_name;
        //  name of the Gazebo Model to be deleted

        public DeleteModelRequest()
        {
            this.model_name = "";
        }

        public DeleteModelRequest(string model_name)
        {
            this.model_name = model_name;
        }

        public static DeleteModelRequest Deserialize(MessageDeserializer deserializer) => new DeleteModelRequest(deserializer);

        private DeleteModelRequest(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.model_name);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.model_name);
        }

        public override string ToString()
        {
            return "DeleteModelRequest: " +
            "\nmodel_name: " + model_name.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
