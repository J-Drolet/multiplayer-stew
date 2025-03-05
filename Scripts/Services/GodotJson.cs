
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