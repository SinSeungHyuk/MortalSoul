using System.Collections.Generic;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using SpineAnimation = Spine.Animation;

namespace MS.EditorTools
{
    public class SpineAnimationInspectorWindow : EditorWindow
    {
        const float DisplayFps = 30f;
        const int PreviewTexSize = 512;

        SkeletonDataAsset dataAsset;
        GameObject previewGO;
        SkeletonAnimation skelAnim;
        PreviewRenderUtility previewUtil;

        string curAnimName;
        string curSkinName;
        float curTime;

        ObjectField dataAssetField;
        PopupField<string> animPopup;
        PopupField<string> skinPopup;
        Image previewImage;
        Slider zoomSlider;
        Slider camYSlider;
        Slider timeSlider;
        Label timeLabel;
        VisualElement timelineMarkers;
        VisualElement eventsContainer;

        [MenuItem("Tools/MS/Spine Animation Inspector")]
        public static void Open()
        {
            var win = GetWindow<SpineAnimationInspectorWindow>();
            win.titleContent = new GUIContent("Spine Animation Inspector");
            win.minSize = new Vector2(540, 740);
            win.Show();
        }

        void OnEnable()
        {
            previewUtil = new PreviewRenderUtility();
            var cam = previewUtil.camera;
            cam.orthographic = true;
            cam.orthographicSize = 3f;
            cam.transform.position = new Vector3(0f, 1f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
        }

        void OnDisable()
        {
            CleanupPreviewGO();
            previewUtil?.Cleanup();
            previewUtil = null;
        }

        void CleanupPreviewGO()
        {
            if (previewGO != null)
            {
                DestroyImmediate(previewGO);
                previewGO = null;
                skelAnim = null;
            }
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingBottom = 8;

            dataAssetField = new ObjectField("Skeleton Data")
            {
                objectType = typeof(SkeletonDataAsset),
                allowSceneObjects = false
            };
            dataAssetField.RegisterValueChangedCallback(_evt =>
            {
                dataAsset = _evt.newValue as SkeletonDataAsset;
                OnDataAssetChanged();
            });
            root.Add(dataAssetField);

            animPopup = new PopupField<string>("Animation", new List<string> { "-" }, 0);
            animPopup.RegisterValueChangedCallback(_evt =>
            {
                curAnimName = _evt.newValue;
                OnAnimationChanged();
            });
            root.Add(animPopup);

            skinPopup = new PopupField<string>("Skin", new List<string> { "-" }, 0);
            skinPopup.RegisterValueChangedCallback(_evt =>
            {
                curSkinName = _evt.newValue;
                ApplyAnimationAtTime();
            });
            root.Add(skinPopup);

            previewImage = new Image
            {
                scaleMode = ScaleMode.ScaleToFit
            };
            previewImage.style.height = 320;
            previewImage.style.marginTop = 8;
            previewImage.style.marginBottom = 4;
            previewImage.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            root.Add(previewImage);

            zoomSlider = new Slider("Zoom", 0.5f, 12f) { value = 3f, showInputField = true };
            zoomSlider.RegisterValueChangedCallback(_evt =>
            {
                if (previewUtil != null) previewUtil.camera.orthographicSize = _evt.newValue;
                RenderPreview();
            });
            root.Add(zoomSlider);

            camYSlider = new Slider("Cam Y", -5f, 5f) { value = 1f, showInputField = true };
            camYSlider.RegisterValueChangedCallback(_evt =>
            {
                if (previewUtil != null)
                {
                    var t = previewUtil.camera.transform;
                    var p = t.position;
                    p.y = _evt.newValue;
                    t.position = p;
                }
                RenderPreview();
            });
            root.Add(camYSlider);

            timeSlider = new Slider("Time", 0f, 1f) { showInputField = true };
            timeSlider.style.marginTop = 10;
            timeSlider.RegisterValueChangedCallback(_evt =>
            {
                curTime = _evt.newValue;
                ApplyAnimationAtTime();
                UpdateTimeLabel();
            });
            root.Add(timeSlider);

            timelineMarkers = new VisualElement();
            timelineMarkers.style.height = 10;
            timelineMarkers.style.marginLeft = 120;
            timelineMarkers.style.marginRight = 24;
            root.Add(timelineMarkers);

            timeLabel = new Label("Time: -");
            timeLabel.style.marginTop = 4;
            timeLabel.style.marginLeft = 120;
            root.Add(timeLabel);

            var eventsHeader = new Label("Events");
            eventsHeader.style.marginTop = 12;
            eventsHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(eventsHeader);

            eventsContainer = new VisualElement();
            eventsContainer.style.marginTop = 4;
            root.Add(eventsContainer);
        }

        void OnDataAssetChanged()
        {
            CleanupPreviewGO();

            if (dataAsset == null)
            {
                animPopup.choices = new List<string> { "-" };
                animPopup.SetValueWithoutNotify("-");
                skinPopup.choices = new List<string> { "-" };
                skinPopup.SetValueWithoutNotify("-");
                timelineMarkers.Clear();
                eventsContainer.Clear();
                previewImage.image = null;
                previewImage.MarkDirtyRepaint();
                curAnimName = null;
                curSkinName = null;
                return;
            }

            previewGO = EditorUtility.CreateGameObjectWithHideFlags(
                "SpineAnimPreview", HideFlags.HideAndDontSave);
            skelAnim = previewGO.AddComponent<SkeletonAnimation>();
            skelAnim.skeletonDataAsset = dataAsset;
            skelAnim.Initialize(true);
            previewUtil.AddSingleGO(previewGO);

            var skeletonData = dataAsset.GetSkeletonData(false);

            var animNames = new List<string>();
            foreach (var a in skeletonData.Animations) animNames.Add(a.Name);
            if (animNames.Count == 0) animNames.Add("-");
            animPopup.choices = animNames;
            animPopup.SetValueWithoutNotify(animNames[0]);
            curAnimName = animNames[0];

            var skinNames = new List<string>();
            foreach (var s in skeletonData.Skins) skinNames.Add(s.Name);
            if (skinNames.Count == 0) skinNames.Add("default");
            skinPopup.choices = skinNames;
            skinPopup.SetValueWithoutNotify(skinNames[0]);
            curSkinName = skinNames[0];

            OnAnimationChanged();
        }

        void OnAnimationChanged()
        {
            if (skelAnim == null || string.IsNullOrEmpty(curAnimName) || curAnimName == "-") return;
            var animation = skelAnim.skeleton.Data.FindAnimation(curAnimName);
            if (animation == null) return;

            timeSlider.lowValue = 0f;
            timeSlider.highValue = Mathf.Max(0.0001f, animation.Duration);
            timeSlider.SetValueWithoutNotify(0f);
            curTime = 0f;

            BuildEventsList(animation);
            BuildTimelineMarkers(animation);
            ApplyAnimationAtTime();
            UpdateTimeLabel();
        }

        void ApplyAnimationAtTime()
        {
            if (skelAnim == null || string.IsNullOrEmpty(curAnimName) || curAnimName == "-") return;
            var skeleton = skelAnim.skeleton;
            var animation = skeleton.Data.FindAnimation(curAnimName);
            if (animation == null) return;

            skeleton.SetToSetupPose();

            if (!string.IsNullOrEmpty(curSkinName))
            {
                var skin = skeleton.Data.FindSkin(curSkinName);
                if (skin != null)
                {
                    skeleton.SetSkin(skin);
                    skeleton.SetSlotsToSetupPose();
                }
            }

            animation.Apply(skeleton, 0f, curTime, false, null, 1f, MixBlend.Setup, MixDirection.In);
            skeleton.Update(0f);
            skeleton.UpdateWorldTransform(Skeleton.Physics.Update);
            skelAnim.LateUpdateMesh();

            RenderPreview();
        }

        void UpdateTimeLabel()
        {
            if (string.IsNullOrEmpty(curAnimName) || curAnimName == "-")
            {
                timeLabel.text = "Time: -";
                return;
            }
            float duration = timeSlider.highValue;
            int frame = Mathf.RoundToInt(curTime * DisplayFps);
            int totalFrames = Mathf.RoundToInt(duration * DisplayFps);
            timeLabel.text = $"Time: {curTime:F4}s / {duration:F4}s   Frame: {frame} / {totalFrames}  @ {DisplayFps}fps";
        }

        void BuildEventsList(SpineAnimation _animation)
        {
            eventsContainer.Clear();
            bool hasAny = false;
            foreach (var tl in _animation.Timelines)
            {
                if (tl is EventTimeline et)
                {
                    for (int i = 0; i < et.Events.Length; i++)
                    {
                        var ev = et.Events[i];
                        var capturedTime = ev.Time;

                        var row = new VisualElement();
                        row.style.flexDirection = FlexDirection.Row;
                        row.style.marginTop = 2;

                        var nameLabel = new Label($"• {ev.Data.Name}");
                        nameLabel.style.width = 180;

                        var infoLabel = new Label($"@ {ev.Time:F4}s  (f{Mathf.RoundToInt(ev.Time * DisplayFps)})");
                        infoLabel.style.width = 180;

                        var goBtn = new Button(() =>
                        {
                            curTime = capturedTime;
                            timeSlider.SetValueWithoutNotify(curTime);
                            ApplyAnimationAtTime();
                            UpdateTimeLabel();
                        })
                        { text = "Go" };
                        goBtn.style.marginLeft = 4;

                        row.Add(nameLabel);
                        row.Add(infoLabel);
                        row.Add(goBtn);
                        eventsContainer.Add(row);
                        hasAny = true;
                    }
                }
            }
            if (!hasAny)
            {
                eventsContainer.Add(new Label("  (no events)"));
            }
        }

        void BuildTimelineMarkers(SpineAnimation _animation)
        {
            timelineMarkers.Clear();
            float duration = _animation.Duration;
            if (duration <= 0f) return;

            foreach (var tl in _animation.Timelines)
            {
                if (tl is EventTimeline et)
                {
                    for (int i = 0; i < et.Events.Length; i++)
                    {
                        var ev = et.Events[i];
                        float t = Mathf.Clamp01(ev.Time / duration);
                        var marker = new VisualElement();
                        marker.style.position = Position.Absolute;
                        marker.style.left = Length.Percent(t * 100f);
                        marker.style.width = 2;
                        marker.style.height = 10;
                        marker.style.backgroundColor = new Color(1f, 0.6f, 0.2f, 1f);
                        marker.tooltip = $"{ev.Data.Name} @ {ev.Time:F4}s";
                        timelineMarkers.Add(marker);
                    }
                }
            }
        }

        void RenderPreview()
        {
            if (previewUtil == null || previewGO == null) return;
            var rect = new Rect(0, 0, PreviewTexSize, PreviewTexSize);
            previewUtil.BeginPreview(rect, GUIStyle.none);
            previewUtil.camera.Render();
            var tex = previewUtil.EndPreview();
            previewImage.image = tex;
            previewImage.MarkDirtyRepaint();
        }
    }
}
