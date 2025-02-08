using Godot;
using Godot.Collections;
using multiplayerstew.Scripts.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace multiplayerstew.Scripts.Services
{
    public partial class MultiplayerPropertyService : Node
    {
        public static MultiplayerPropertyService Instance { get; private set; }
        public override void _Ready()
        {
            Instance = this;
        }

        public static object ConvertVariantToType(Variant variant)
        {
            switch (variant.VariantType)
            {
                case Variant.Type.Bool:
                    return variant.AsBool();
                case Variant.Type.Int:
                    return variant.AsInt32();
                case Variant.Type.Float:
                    return variant.AsSingle();
                case Variant.Type.String:
                    return variant.AsString();
                case Variant.Type.Vector2:
                    return variant.AsVector2();
                case Variant.Type.Vector3:
                    return variant.AsVector3();
                case Variant.Type.Color:
                    return variant.AsColor();
                case Variant.Type.NodePath:
                    return variant.AsNodePath();
                case Variant.Type.Object:
                    return variant.AsGodotObject();
                default:
                    GD.PrintErr($"Unsupported Variant type: {variant.VariantType}");
                    return null;
            }
        }

        private static void SetProperty(NodePath nodePath, string propertyName, Variant propertyValue)
        {   
            Node node = Instance.GetNodeOrNull(nodePath);

            if(node != null)
            {
                Array<Dictionary> propertyList = node.GetPropertyList();

                foreach(Dictionary property in propertyList)
                {
                    if(property["name"].ToString().ToLower() == propertyName.ToLower() && (Variant.Type)(int)property["type"] == propertyValue.VariantType)
                    { 
                        PropertyInfo propertyInfo = node.GetType().GetProperty(propertyName);
                        if(propertyInfo != null)
                        {
                            propertyInfo.SetValue(node, ConvertVariantToType(propertyValue));
                            break;
                        }
                    }
                }
            }
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
        public void SetPropertyUnreliable(NodePath nodePath, string propertyName, Variant propertyValue)
        {
            SetProperty(nodePath, propertyName, propertyValue);
        }

        [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
        public void SetPropertyReliable(NodePath nodePath, string propertyName, Variant propertyValue)
        {   
            SetProperty(nodePath, propertyName, propertyValue);
        }
    }

}