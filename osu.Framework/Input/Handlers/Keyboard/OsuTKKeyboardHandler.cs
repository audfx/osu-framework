﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.StateChanges;
using osu.Framework.Platform;
using osu.Framework.Statistics;
using osuTK.Input;

namespace osu.Framework.Input.Handlers.Keyboard
{
    internal class OsuTKKeyboardHandler : InputHandler
    {
        public override bool IsActive => true;

        public override int Priority => 0;

        private TkKeyboardState lastEventState;
        private KeyboardState? lastRawState;

        public override bool Initialize(GameHost host)
        {
            Enabled.BindValueChanged(e =>
            {
                if (e.NewValue)
                {
                    host.Window.KeyDown += handleKeyboardEvent;
                    host.Window.KeyUp += handleKeyboardEvent;
                }
                else
                {
                    host.Window.KeyDown -= handleKeyboardEvent;
                    host.Window.KeyUp -= handleKeyboardEvent;
                    lastRawState = null;
                    lastEventState = null;
                }
            }, true);

            return true;
        }

        private void handleKeyboardEvent(object sender, KeyboardKeyEventArgs e)
        {
            var rawState = e.Keyboard;

            if (lastRawState != null && rawState.Equals(lastRawState))
                return;

            lastRawState = rawState;

            var newState = new TkKeyboardState(rawState);

            PendingInputs.Enqueue(new KeyboardKeyInput(newState.Keys, lastEventState?.Keys));

            lastEventState = newState;

            FrameStatistics.Increment(StatisticsCounterType.KeyEvents);
        }

        private class TkKeyboardState : States.KeyboardState
        {
            private static readonly IEnumerable<Key> all_keys = Enum.GetValues(typeof(Key)).Cast<Key>();

            public TkKeyboardState(KeyboardState tkState)
            {
                if (tkState.IsAnyKeyDown)
                {
                    foreach (var key in all_keys)
                    {
                        if (tkState.IsKeyDown(key))
                        {
                            Keys.SetPressed(key, true);
                        }
                    }
                }
            }
        }
    }
}
