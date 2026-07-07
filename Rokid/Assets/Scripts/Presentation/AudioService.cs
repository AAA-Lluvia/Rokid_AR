using UnityEngine;
using FocusWorld.Core;

namespace FocusWorld.Presentation
{
    /// <summary>
    /// 音频服务（升级版）。只订阅事件，不含逻辑。由 B 负责。
    /// 吃三个时机：
    ///   TaskStarted     → 播开场语音（“请从 1 开始按顺序寻找”）
    ///   SelectionResult → 正确/错误音效；错误时可按 errorType 播不同提示（二期）
    ///   TaskCompleted   → 完成音效
    /// 任一 clip 为空则跳过，第一天没音源也不报错。
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        [SerializeField] AudioSource source;

        [Header("Clips")]
        [SerializeField] AudioClip startClip;
        [SerializeField] AudioClip correctClip;
        [SerializeField] AudioClip wrongClip;
        [SerializeField] AudioClip completeClip;

        [Header("二期：镜像/旋转错误的专属语音（可留空回退到 wrongClip）")]
        [SerializeField] AudioClip mirrorHintClip;   // b↔d、p↔q
        [SerializeField] AudioClip rotationHintClip; // 6↔9

        void OnEnable()
        {
            EventBus.Subscribe(GameEvent.TaskStarted, OnStarted);
            EventBus.Subscribe(GameEvent.SelectionResult, OnResult);
            EventBus.Subscribe(GameEvent.TaskCompleted, OnCompleted);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe(GameEvent.TaskStarted, OnStarted);
            EventBus.Unsubscribe(GameEvent.SelectionResult, OnResult);
            EventBus.Unsubscribe(GameEvent.TaskCompleted, OnCompleted);
        }

        void OnStarted(object _) => Play(startClip);

        void OnResult(object payload)
        {
            if (payload is not SelectionResultEvent e) return;
            if (e.correct) { Play(correctClip); return; }
            Play(PickWrongClip(e.errorType));
        }

        void OnCompleted(object _) => Play(completeClip);

        AudioClip PickWrongClip(ErrorType type) => type switch
        {
            ErrorType.MirrorConfusion or ErrorType.VerticalFlip
                when mirrorHintClip != null => mirrorHintClip,
            ErrorType.RotationConfusion
                when rotationHintClip != null => rotationHintClip,
            _ => wrongClip, // 一期恒走这里（errorType = Wrong）
        };

        void Play(AudioClip clip)
        {
            if (source != null && clip != null) source.PlayOneShot(clip);
        }
    }
}
