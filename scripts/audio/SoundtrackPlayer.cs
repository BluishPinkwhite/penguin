using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Incremental.scripts.director;
using Incremental.scripts.director.data;

public partial class SoundtrackPlayer : AudioStreamPlayer2D
{
    [Export] private Array<AudioStream> tracks;
    [Export] private Array<bool> trackUnlocks;
    [Export] private float initialDelay = 10f;
    [Export] private float fadeInTime = 7f;

    private float _busVolumeDb = -80f;

    private bool _isFadedIn;
    private bool _firstSong;

    public override void _Ready()
    {
        if (tracks.Count == 0)
        {
            GD.PrintErr("No tracks provided for SoundtrackPlayer");
            return;
        }

        trackUnlocks[0] = true;

        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("BackgroundMusic"), _busVolumeDb);


        Finished += PlayRandomTrack;
        PlayRandomTrack();
    }

    private async void PlayRandomTrack()
    {
        if (Inventory.Items[Item.Archeologist].Amount + Inventory.Items[Item.Miner].Amount +
            Inventory.Items[Item.Hauler].Amount > 30)
        {
            trackUnlocks[1] = true;
        }

        for (int i = 0; i < trackUnlocks.Count; i++)
        {
            if (trackUnlocks[i])
                GD.Print("Unlocked track: " + i);
        }

        int randomIndex = 0;
        AudioStream trackToPlay;
        do
        {
            randomIndex = GD.RandRange(0, tracks.Count - 1);
            trackToPlay = tracks[randomIndex];
        } while (!trackUnlocks[randomIndex]);

        if (!_firstSong)
            await ToSignal(GetTree().CreateTimer(initialDelay), SceneTreeTimer.SignalName.Timeout);

        SetStream(trackToPlay);
        Play();
        FadeIn(fadeInTime);
        _firstSong = true;
    }

    private void FadeIn(float time)
    {
        if (_isFadedIn) return;
        _isFadedIn = true;
        int busIndex = AudioServer.GetBusIndex("BackgroundMusic");

        Tween fadeInTween = CreateTween();

        fadeInTween.TweenMethod(
            Callable.From<float>((value) =>
            {
                _busVolumeDb = value;
                AudioServer.SetBusVolumeDb(busIndex, _busVolumeDb);
            }),
            -80f,
            -6f,
            time * Engine.TimeScale
        );
    }
}