using System;
using System.Collections.Generic;
using System.Windows.Forms;

using ScriptPortal.Vegas;

public class EntryPoint
{
    Vegas vegas;

    public void FromVegas(Vegas v)
    {
        vegas = v;

        VideoEvent ev = GetSelectedEvent();
        if (ev == null) return;

        Timecode cursor = ev.Start;

        // === 1. Duration 1 ===
        cursor = Slice(ev, cursor, 1.0);

        // === 2. Duration 1 Reverse ===
        ApplyPlayback(ev, -1.0);
        cursor = Slice(ev, cursor, 1.0);
        ApplyPlayback(ev, 1.0);

        // === 3. Pinch/Punch max (0.1) ===
        ApplyFX(ev, "Pinch/Punch");
        cursor = Slice(ev, cursor, 0.1);

        // === 4. 30 FX spam (0.04 x 30) ===
        RepeatFX(ev, cursor, 0.04, 30, "Gaussian Blur");
        cursor += Timecode.FromSeconds(0.04 * 30);

        // === 5. Speed 4 (0.03 x 60) ===
        RepeatSpeed(ev, cursor, 0.03, 60, 4.0);
        cursor += Timecode.FromSeconds(0.03 * 60);

        // === 6. 40 FX spam (0.02 x 138) ===
        RepeatFX(ev, cursor, 0.02, 138, "Invert");
        cursor += Timecode.FromSeconds(0.02 * 138);

        // === 7. TV Simulator Line Sync (2 sec) ===
        ApplyTVLineSync(ev, cursor, 2.0);

        // === 8. Speed 4 (0.03 x 100) ===
        RepeatSpeed(ev, cursor, 0.03, 100, 4.0);
        cursor += Timecode.FromSeconds(0.03 * 100);

        // === 9. 40 FX spam (0.02 x 100) ===
        RepeatFX(ev, cursor, 0.02, 100, "Invert");
        cursor += Timecode.FromSeconds(0.02 * 100);

        // === 10. HSL Hue 0 → 1 + TV Vertical Sync (3 sec) ===
        ApplyHSLHue(ev, cursor, 3.0);
        ApplyTVVerticalSync(ev, cursor, 3.0);
        cursor += Timecode.FromSeconds(3);

        // === 11. Speed 4 (0.03 x 60) ===
        RepeatSpeed(ev, cursor, 0.03, 60, 4.0);
        cursor += Timecode.FromSeconds(0.03 * 60);

        // === 12. Flip Horizontal spam (0.02 x 40) ===
        RepeatFX(ev, cursor, 0.02, 40, "Flip Horizontal");
    }

    // ================= HELPERS =================

    VideoEvent GetSelectedEvent()
    {
        foreach (Track t in vegas.Project.Tracks)
            foreach (TrackEvent e in t.Events)
                if (e.Selected && e is VideoEvent)
                    return (VideoEvent)e;
        return null;
    }

    Timecode Slice(VideoEvent ev, Timecode start, double sec)
    {
        Timecode len = Timecode.FromSeconds(sec);
        ev.Split(start + len);
        return start + len;
    }

    void ApplyPlayback(VideoEvent ev, double rate)
    {
        ev.PlaybackRate = rate;
    }

    void ApplyFX(VideoEvent ev, string fxName)
    {
        Effect fx = vegas.VideoFX.GetChildByName(fxName);
        if (fx != null)
            ev.Effects.Add(new Effect(fx));
    }

    void RepeatFX(VideoEvent ev, Timecode start, double sec, int count, string fx)
    {
        for (int i = 0; i < count; i++)
        {
            ApplyFX(ev, fx);
            Slice(ev, start + Timecode.FromSeconds(sec * i), sec);
        }
    }

    void RepeatSpeed(VideoEvent ev, Timecode start, double sec, int count, double speed)
    {
        for (int i = 0; i < count; i++)
        {
            ApplyPlayback(ev, speed);
            Slice(ev, start + Timecode.FromSeconds(sec * i), sec);
        }
        ApplyPlayback(ev, 1.0);
    }

    void ApplyTVLineSync(VideoEvent ev, Timecode start, double sec)
    {
        Effect fx = vegas.VideoFX.GetChildByName("TV Simulator");
        if (fx == null) return;

        Effect inst = new Effect(fx);
        ev.Effects.Add(inst);

        OFXParameter p = inst.OFXEffect.Parameters["Line Sync"];
        p.SetValueAtTime(start, 1.0);
        p.SetValueAtTime(start + Timecode.FromSeconds(sec), 0.0);
    }

    void ApplyTVVerticalSync(VideoEvent ev, Timecode start, double sec)
    {
        Effect fx = vegas.VideoFX.GetChildByName("TV Simulator");
        if (fx == null) return;

        Effect inst = new Effect(fx);
        ev.Effects.Add(inst);

        OFXParameter p = inst.OFXEffect.Parameters["Vertical Sync"];
        p.SetValueAtTime(start, 1.0);
        p.SetValueAtTime(start + Timecode.FromSeconds(sec), 0.0);
    }

    void ApplyHSLHue(VideoEvent ev, Timecode start, double sec)
    {
        Effect fx = vegas.VideoFX.GetChildByName("HSL Adjust");
        if (fx == null) return;

        Effect inst = new Effect(fx);
        ev.Effects.Add(inst);

        OFXParameter hue = inst.OFXEffect.Parameters["Hue"];
        hue.SetValueAtTime(start, 0.0);
        hue.SetValueAtTime(start + Timecode.FromSeconds(sec), 1.0);
    }
}
