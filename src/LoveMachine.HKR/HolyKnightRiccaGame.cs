﻿using HarmonyLib;
using LoveMachine.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace LoveMachine.HKR
{
    internal class HolyKnightRiccaGame : GameDescriptor
    {
        private PlayableDirector director;
        private TimelineAsset timeline;
        private Traverse<string> cutName;
        private Dictionary<string, TimelineClip> clipCache;
        private TimeUnlooper unlooper;

        public void StartH(MonoBehaviour uiController)
        {
            cutName = Traverse.Create(uiController)
                .Property("actorController")
                .Property<string>("currentCutName");
            StartH();
        }

        public override int AnimationLayer => 0;

        protected override Dictionary<Bone, string> FemaleBoneNames => new Dictionary<Bone, string>
        {
            { Bone.Vagina, "DEF-clitoris" },
            { Bone.Mouth, "MouseTransform" },
            { Bone.RightHand, "DEF-f_index_01_R" },
            { Bone.LeftHand, "DEF-f_index_01_L" },
            { Bone.RightBreast, "DEF-nipple_R" },
            { Bone.LeftBreast, "DEF-nipple_L" }
        };

        protected override int HeroineCount => 1;

        protected override int MaxHeroineCount => 1;

        protected override bool IsHardSex => true;

        protected override bool IsHSceneInterrupted => false;

        public override Animator GetFemaleAnimator(int girlIndex) =>
            throw new NotImplementedException();

        protected override Transform GetDickBase() =>
            new[] { "DEF-testicle", "ORG-testicle" }
                .Select(GameObject.Find)
                .First(go => go != null)
                .transform;

        protected override GameObject GetFemaleRoot(int girlIndex) =>
            GameObject.Find("ricasso/root");

        protected override string GetPose(int girlIndex) => cutName.Value;

        protected override bool IsIdle(int girlIndex) => false;

        protected override void GetAnimState(int girlIndex, out float normalizedTime,
            out float length, out float speed)
        {
            string pose = GetPose(0);
            if (!clipCache.TryGetValue(pose, out var clip))
            {
                clip = GetCurrentClip();
                if (clip == null)
                {
                    normalizedTime = 0f;
                    length = 1f;
                    speed = 1f;
                    return;
                }
                clipCache[pose] = clip;
            }
            float time = (float)((director.time - clip.start) / clip.duration);
            normalizedTime = unlooper.LoopingToMonotonous(time);
            length = (float)clip.duration;
            speed = 1f;
        }

        private TimelineClip? GetCurrentClip() => Enumerable.Range(0, timeline.outputTrackCount)
            .Select(timeline.GetOutputTrack)
            .Where(track => director.GetGenericBinding(track)?.name == "RicassoKnightActor_atd")
            .SelectMany(track => track.clips)
            .Where(clip => clip.displayName.StartsWith("SimpleCharacterAnimationClipPlayableAsset"))
            .Where(clip => clip.start < director.time && director.time < clip.end)
            .FirstOrDefault();

        protected override IEnumerator UntilReady()
        {
            yield return new WaitForSeconds(5f);
            director = GameObject.Find("ATDTimeline").GetComponent<PlayableDirector>();
            timeline = director.playableAsset.Cast<TimelineAsset>();
            clipCache = new Dictionary<string, TimelineClip>();
            unlooper = new TimeUnlooper();
        }
    }
}