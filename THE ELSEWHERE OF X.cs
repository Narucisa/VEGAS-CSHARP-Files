using System;
using Sony.Vegas;

public class EntryPoint
{
    public void FromVegas(Vegas vegas)
    {
        TrackEvent ev = vegas.Project.Tracks[0].Events[0];

        // 1. Reverse
        ev.Reverse = true;
        ev.Length = Timecode.FromSeconds(1);

        // 2. Velocity Envelope
        Envelope vel = new Envelope(EnvelopeType.Velocity);
        ev.Envelopes.Add(vel);

        vel.Points.Add(new EnvelopePoint(ev.Start, 4.0f));
        vel.Points.Add(new EnvelopePoint(ev.End, 4.0f));

        // 3. Speed Spam Loop
        RepeatVelocitySpam(ev, 60, 0.03);
        RepeatVelocitySpam(ev, 138, 0.02);

        // 4. FX placeholders
        AddFX(ev, "Pinch/Punch");
        AddFX(ev, "TV Simulator");
        AddFX(ev, "HSL Adjust");
        AddFX(ev, "Wave");
        AddFX(ev, "Newsprint");
        AddFX(ev, "Swirl");
        AddFX(ev, "Flip Vertical");
        AddFX(ev, "Channel Blend");
        AddFX(ev, "Deform");
        AddFX(ev, "Flip Horizontal");

        // 5. TV Sync keyframe simulation (manual tweak needed)
        Console.WriteLine("Script Generate");
    }

    void RepeatVelocitySpam(TrackEvent ev, int repeats, double dur)
    {
        Timecode t = ev.Start;
        for (int i = 0; i < repeats; i++)
        {
            ev.Split(t + Timecode.FromSeconds(dur));
            t += Timecode.FromSeconds(dur);
        }
    }

    void AddFX(TrackEvent ev, string fxName)
    {
        foreach (Effect fx in ev.Effects)
            if (fx.PlugIn.Name == fxName) return;

        PlugInNode plug = ev.Media.Generator.PlugIn;
        if (plug != null)
            ev.Effects.Add(new Effect(plug));
    }
}
