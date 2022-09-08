using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TargetController : Singleton<TargetController>
{
    protected override bool dont_destroy_on_load { get; set; } = false;

    [SerializeField] GameObject targetPrefab;

    List<Target> allTargets = new List<Target>();
    public Target current_ready_target { get; private set; }
    public Target current_focusing_target { get; private set; }
    public List<Target> current_selecting_targets { get; private set; } = new List<Target>();

    [SerializeField] Color readyColor, focusColor, selectColor;
    [SerializeField] float moveDuration, colorDuration;
    [SerializeField] float offset;

    Sequence seq;

    Target GetTarget()
    {
        Target result = null;
        if (allTargets.Count == 0)
        {
            Target new_target = Instantiate(targetPrefab, transform).GetComponent<Target>();
            allTargets.Add(new_target);
            result = new_target;
        }
        else
        {
            foreach (Target target in allTargets)
            {
                if (target.targetState == TargetState.NOT_ACTIVE)
                {
                    result = target;
                    break;
                }
            }
            if (!result)
            {
                Target new_target = Instantiate(targetPrefab, transform).GetComponent<Target>();
                allTargets.Add(new_target);
                result = new_target;
            }
        }
        return result;
    }

    public void ActivateTarget()
    {
        Target target = GetTarget();
        Transform device_trans = DeviceInfo.I.transform;

        seq = DOTween.Sequence();
        seq.OnStart(() =>
        {
            target.transform.SetParent(device_trans, false);
            target.transform.localPosition = new Vector3(0, -2.5f, offset);
            target.image.color = readyColor;
        });
        seq.Append(target.transform.DOLocalMoveY(0, moveDuration));
        seq.OnComplete(() =>
        {
            target.ChangeTargetState(TargetState.READY);
            current_ready_target = target;
        });
        seq.Play();
    }

    public void ReadyTarget()
    {
        if (!current_focusing_target) return;
        Transform device_trans = DeviceInfo.I.transform;

        if (seq.IsActive() && seq.IsPlaying()) seq.Kill(true);
        seq = DOTween.Sequence();
        seq.OnStart(() => current_focusing_target.transform.SetParent(device_trans, false));
        seq.Append(current_focusing_target.transform.DOLocalMove(new Vector3(0, 0, offset), moveDuration));
        seq.Join(current_focusing_target.image.DOColor(readyColor, colorDuration));
        seq.OnComplete(() =>
        {
            current_focusing_target.ChangeTargetState(TargetState.READY);
            current_focusing_target.focusingObject = null;
            current_ready_target = current_focusing_target;
            this.current_focusing_target = null;
        });
        seq.Play();
    }

    public void FocusTarget(GameObject obj)
    {
        Target target;
        if (current_ready_target) target = current_ready_target;
        else if (current_focusing_target) target = current_focusing_target;
        else return;

        if (seq.IsActive() && seq.IsPlaying()) seq.Kill(false);
        seq = DOTween.Sequence();
        seq.OnStart(() => target.transform.SetParent(obj.transform, true));
        seq.Append(target.transform.DOLocalMove(Vector3.zero, moveDuration));
        seq.Join(target.image.DOColor(focusColor, colorDuration));
        seq.OnComplete(() =>
        {
            target.ChangeTargetState(TargetState.FOCUSING);
            target.focusingObject = obj;
            current_focusing_target = target;
            current_ready_target = null;
        });
        seq.Play();
    }

    public void SelectTarget(GameObject obj)
    {
        if (!current_focusing_target) return;
        Target cft = current_focusing_target;

        if (seq.IsActive() && seq.IsPlaying()) seq.Kill(true);
        seq = DOTween.Sequence();
        seq.Append(cft.image.DOColor(selectColor, colorDuration));
        seq.OnComplete(() =>
        {
            cft.ChangeTargetState(TargetState.SELECTING);
            cft.selectingObject = obj;
            cft.focusingObject = null;
            current_selecting_targets.Add(cft);
            current_focusing_target = null;
        });
        seq.Play();
    }

    public void DeactivateTarget(Target target)
    {
        if (!target) return;
        target.ChangeTargetState(TargetState.NOT_ACTIVE);
        target.image.color = Color.clear;
        if (current_ready_target == target) current_ready_target = null;
        if (current_focusing_target == target) current_focusing_target = null;
    }

    public void DeactivateAllTargets()
    {
        if (allTargets.Count == 0) return;
        foreach (Target target in allTargets) DeactivateTarget(target);
    }

    public void VisualizeTarget(Target target, bool visible)
    {
        if (visible)
        {
            switch (target.targetState)
            {
                case TargetState.READY: target.image.color = readyColor; break;
                case TargetState.FOCUSING: target.image.color = focusColor; break;
                case TargetState.SELECTING: target.image.color = selectColor; break;
            }
        }
        else target.image.color = Color.clear;
    }
}
