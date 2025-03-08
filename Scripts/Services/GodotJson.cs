
using System;
using System.Text.Json;
using Godot;

/// <summary>
/// This service is used when you want to pass JSON in a Godot Node Name attribute. Since the name escapes necessary JSON characters we need to replace them on both ends
/// </summary>
public static class GodotJson
{

    /// <summary>
    /// JsonSerializer does not serialize Godot.Vector3 so this is a wrapper just for transport reasons
    /// </summary>
    public struct SerializableVector3 {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public SerializableVector3(Vector3 vector) 
        {
            x = vector.X;
            y = vector.Y;
            z = vector.Z;
        }

        public Vector3 ToVector3() 
        {
            return new Vector3(x, y, z);
        }
    }

    public struct SerializableQuaternion  {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }

        public SerializableQuaternion(Quaternion quaternion) 
        {
            x = quaternion.X;
            y = quaternion.Y;
            z = quaternion.Z;
            w = quaternion.W;
        }

        public Quaternion ToQuaternion() 
        {
            return new Quaternion(x, y, z, w);
        }
    }

    public struct SerializableBasis {
        public SerializableQuaternion quaternion { get; set; }

        public SerializableBasis(Basis basis) 
        {
            quaternion = new SerializableQuaternion(basis.GetRotationQuaternion());
        }

        public Basis ToBasis() 
        {
            return new Basis(quaternion.ToQuaternion());
        }
    }

    public struct SerializableTransform3D {
        public SerializableBasis basis { get; set; }
        public SerializableVector3 origin { get; set; }

        public SerializableTransform3D(Transform3D transform3D) 
        {
            basis = new SerializableBasis(transform3D.Basis);
            origin = new SerializableVector3(transform3D.Origin);
        }

        public Transform3D ToTransform3D() 
        {
            return new Transform3D(basis.ToBasis(), origin.ToVector3());
        }
    }

    public static T FromJson<T>(string base64String)
    {
        string decodedJson = Convert.FromBase64String(base64String).GetStringFromUtf8();
        return JsonSerializer.Deserialize<T>(decodedJson);
    }

    public static string ToJson<T>(T obj)
    {
        string json =  JsonSerializer.Serialize(obj);
        return Convert.ToBase64String(json.ToUtf8Buffer());
    }
}