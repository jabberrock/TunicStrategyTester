using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace TunicStrategyTester
{
    internal class Attempt
    {
        public class PlayerPose
        {
            public Vector3 Position { get; set; }
            public Quaternion Rotation { get; set; }
        }

        private class Sample : IComparable<Sample>
        {
            public double Time { get; set; }
            public Vector3 Position { get; set; }
            public Quaternion Rotation { get; set; }

            public int CompareTo(Sample other)
            {
                return this.Time.CompareTo(other.Time);
            }

            public PlayerPose ToPlayerPose()
            {
                return new PlayerPose()
                {
                    Position = this.Position,
                    Rotation = this.Rotation
                };
            }
        }

        private readonly List<Sample> samples = new List<Sample>();

        public void AddSample(double time, Vector3 position, Quaternion rotation)
        {
            var lastSample = this.samples.LastOrDefault();

            // Sometimes we try to add samples before the scene has finished
            // loading, and the in-game time is still from the previous
            // attempt. So if we detect going back in time, we just clear out
            // the samples and start again.
            if (lastSample != null && time < lastSample.Time)
            {
                this.samples.Clear();
            }

            if (lastSample == null || time > lastSample.Time)
            {
                this.samples.Add(new Sample()
                {
                    Time = time,
                    Position = position,
                    Rotation = rotation
                });
            }
        }

        public PlayerPose At(double time)
        {
            if (this.samples.Count == 0)
            {
                return null;
            }

            var index = this.samples.BinarySearch(new Sample() { Time = time });
            if (index >= 0)
            {
                return this.samples[index].ToPlayerPose();
            }
            else
            {
                index = ~index;
                if (index < this.samples.Count)
                {
                    return this.samples[index].ToPlayerPose();
                }
                else
                {
                    return this.samples.Last().ToPlayerPose();
                }
            }
        }

        public void MarkComplete()
        {
            this.IsComplete = true;
        }

        public bool IsComplete { get; private set; }

        public double? CompletedDuration()
        {
            if (this.IsComplete && this.samples.Count > 0)
            {
                return this.samples.Last().Time - this.samples.First().Time;
            }
            else
            {
                return null;
            }
        }
    }
}
