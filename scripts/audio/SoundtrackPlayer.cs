using Godot;
using System;
using Godot.Collections;

public partial class SoundtrackPlayer : AudioStreamPlayer2D
{
	[Export] private Array<AudioStream> tracks;
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
		
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("BackgroundMusic"), _busVolumeDb);
		
		
		Finished += PlayRandomTrack;
		PlayRandomTrack();
	}
	
	private async void PlayRandomTrack()
	{
		int randomIndex = GD.RandRange(0, tracks.Count - 1);
		AudioStream randomTrack = tracks[randomIndex];
		
		if(!_firstSong)
			await ToSignal(GetTree().CreateTimer(initialDelay), SceneTreeTimer.SignalName.Timeout);
		
		SetStream(randomTrack);
		Play();
		FadeIn(fadeInTime);
		_firstSong = true;
	}

	private void FadeIn(float time)
	{
		if(_isFadedIn) return;
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
