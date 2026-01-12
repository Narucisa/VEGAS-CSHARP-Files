using System;
using Sony.Vegas;

public class EntryPoint
{
    Vegas vegas;

    public void FromVegas(Vegas v)
    {
        vegas = v;

        VideoEvent ev = GetSelectedVideoEvent();
        if (ev == null) return;

        Timecode cursor = ev.Start;

        Split(ev, 1.0, ref cursor);                     // duration 1
        SplitReverse(ev, 1.0, ref cursor);              // duration 1 reverse
        SplitPinch(ev, 0.1, ref cursor);                // pinch/punch max
        Split(ev, 1.0, ref cursor);

        RepeatFX(ev, 0.04, 30, ref cursor);
        RepeatSpeed(ev, 0.03, 60, ref cursor);
        RepeatFX(ev, 0.02, 138, ref cursor);

        SplitTVLineSync(ev, 2.0, ref cursor);

        RepeatSpeed(ev, 0.03, 100, ref cursor);
        RepeatFX(ev, 0.02, 100, ref cursor);

        SplitHSL_TV(ev, 3.0, ref cursor);

        RepeatSpeed(ev, 0.03, 60, ref cursor);
        RepeatFX(ev, 0.02, 138, ref cursor);
        RepeatSpeed(ev, 0.03, 60, ref cursor);
        RepeatFlip(ev, 0.02, 40, ref cursor);
    }

    // ================= HELPERS =================

    VideoEvent GetSelectedVideoEvent()
    {
        foreach (Track t in vegas.Project.Tracks)
            if (t.IsVideo())
                foreach (TrackEvent e in t.Events)
                    if (e.Selected)
                        return e as VideoEvent;
        return null;
    }

    void Split(VideoEvent ev, double sec, ref Timecode pos)
    {
        ev.Split(pos + Timecode.FromSeconds(sec));
        pos += Timecode.FromSeconds(sec);
    }

    void SplitReverse(VideoEvent ev, double sec, ref Timecode pos)
    {
        Split(ev, sec, ref pos);
        ev.PlaybackRate = -1.0;
    }

    void SplitPinch(VideoEvent ev, double sec, ref Timecode pos)
    {
        Split(ev, sec, ref pos);
        ApplyFX(ev, "Pinch/Punch", "Amount", 1.0);
    }

    void SplitTVLineSync(VideoEvent ev, double sec, ref Timecode pos)
    {
        Split(ev, sec, ref pos);
        Effect fx = ApplyFX(ev, "TV Simulator", "Line Sync", 1.0);
        fx.ParameterByName("Line Sync").Keyframes.Add(
            new Keyframe(ev.Length, 0.0)
        );
    }

    void SplitHSL_TV(VideoEvent ev, double sec, ref Timecode pos)
    {
        Split(ev, sec, ref pos);
        Effect hsl = ApplyFX(ev, "HSL Adjust", "Hue", 0.0);
        hsl.ParameterByName("Hue").Keyframes.Add(
            new Keyframe(ev.Length, 1.0)
        );
        hsl.ParameterByName("Saturation").Value = 1.0;

        Effect tv = ApplyFX(ev, "TV Simulator", "Vertical Sync", 1.0);
        tv.ParameterByName("Vertical Sync").Keyframes.Add(
            new Keyframe(ev.Length, 0.0)
        );
    }

    void RepeatFX(VideoEvent ev, double sec, int count, ref Timecode pos)
    {
        for (int i = 0; i < count; i++)
        {
            Split(ev, sec, ref pos);
            ApplyFX(ev, "Gaussian Blur", "Amount", 0.2);
        }
    }

    void RepeatSpeed(VideoEvent ev, double sec, int count, ref Timecode pos)
    {
        for (int i = 0; i < count; i++)
        {
            Split(ev, sec, ref pos);
            ev.PlaybackRate = 4.0;
        }
    }

    void RepeatFlip(VideoEvent ev, double sec, int count, ref Timecode pos)
    {
        for (int i = 0; i < count; i++)
        {
            Split(ev, sec, ref pos);
            ApplyFX(ev, "Mirror", "Horizontal", 1.0);
        }
    }

    Effect ApplyFX(VideoEvent ev, string fxName, string param, double value)
    {
        PlugInNode node = vegas.VideoFX.GetChildByName(fxName);
        Effect fx = new Effect(node);
        ev.Effects.Add(fx);
        fx.ParameterByName(param).Value = value;
        return fx;
    }
}
