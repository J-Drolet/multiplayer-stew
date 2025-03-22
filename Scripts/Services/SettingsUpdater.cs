using System;
using System.Collections.Generic;
using Godot;

public partial class SettingsUpdater: Node 
{
    public override void _Ready()
    {
        Config.Instance.ConfigChanged += SettingsUpdated;

        Config.EmitInitialSettings();
    }

    private void SettingsUpdated(string section, string propertyKey, Variant propertyValue)
    {

        switch(section) {
            case "settings":
                switch(propertyKey) {
                    case "display_mode":
                        switch((int) propertyValue) {
                            case 0:
                                DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
                                break;
                            case 1:
                                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                                break;
                            case 2:
                                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                                break;
                        }
                        break;
                    case "music_volume":
                        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Mathf.LinearToDb((float) propertyValue));
                        break;
                    case "sfx_volume":
                        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), Mathf.LinearToDb((float) propertyValue));
                        break;
                    case "antialiasing_mode":
                        GetViewport().Msaa3D = (Viewport.Msaa)(int)propertyValue;
                        break;
                    case "anisotropic_filtering":
                        GetViewport().AnisotropicFilteringLevel = (Viewport.AnisotropicFiltering )(int)propertyValue;
                        break;
                    case "scaling_mode":
                        switch((int) propertyValue) {
                            case 0: // no upscaling
                                GetViewport().Scaling3DMode = Viewport.Scaling3DModeEnum.Bilinear;
                                GetViewport().Scaling3DScale = 1.0f;
                                break;
                            case 1: // Ultra Quality
                                GetViewport().Scaling3DMode = Viewport.Scaling3DModeEnum.Fsr2;
                                GetViewport().Scaling3DScale = 0.77f;
                                break;
                            case 2: // Quality
                                GetViewport().Scaling3DMode = Viewport.Scaling3DModeEnum.Fsr2;
                                GetViewport().Scaling3DScale = 0.67f;
                                break;
                            case 3: // Balanced
                                GetViewport().Scaling3DMode = Viewport.Scaling3DModeEnum.Fsr2;
                                GetViewport().Scaling3DScale = 0.59f;
                                break;
                            case 4: // Performance
                                GetViewport().Scaling3DMode = Viewport.Scaling3DModeEnum.Fsr2;
                                GetViewport().Scaling3DScale = 0.5f;
                                break;
                            case 5: // Near-sighted
                                GetViewport().Scaling3DMode = Viewport.Scaling3DModeEnum.Fsr2;
                                GetViewport().Scaling3DScale = 0.1f;
                                break;
                        }
                        break;
                    case "ambient_occlusion":
                        if(LevelManager.Instance != null)
                        {
                            List<WorldEnvironment> worldEnvironments = GodotNodeFindingService.FindNodes<WorldEnvironment>(LevelManager.Instance);
                            foreach(WorldEnvironment worldEnvironment in worldEnvironments)
                            {
                                Godot.Environment environment = worldEnvironment.Environment;
                                switch((int) propertyValue) {
                                    case 0: // none
                                        environment.SsaoEnabled = false;
                                        break;
                                    case 2: // medium
                                        environment.SsaoEnabled = true;
                                        environment.SsaoRadius = 1.0f;
                                        break;
                                    case 3: // high
                                        environment.SsaoEnabled = true;
                                        environment.SsaoRadius = 5.0f;
                                        break;
                                }
                            }
                        }
                        break;
                    }
                break;
        } 
    }
}