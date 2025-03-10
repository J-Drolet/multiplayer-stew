using Godot;
using multiplayerstew.Scripts.Services;
using System;

public partial class LoadingScreen : Control
{

    public override void _Ready()
    {
        GodotErrorService.ValidateRequiredData(this);
        UI.LoadingScreen = this;
    }
}
