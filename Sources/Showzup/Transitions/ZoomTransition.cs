using Silphid.Sequencit;
using Silphid.Tweenzup;
using UniRx;
using UnityEngine;

namespace Silphid.Showzup
{
    public class ZoomTransition : CrossfadeTransition
    {
        public float StartScale { get; set; } = 0.8f;
        public float EndScale { get; set; } = 1.2f;

        public override void Prepare(GameObject sourceContainer, GameObject targetContainer, IOptions options)
        {
            base.Prepare(sourceContainer, targetContainer, options);

            if (sourceContainer != null)
                ((RectTransform) sourceContainer.transform).localScale = Vector3.one;

            var scale = options.GetDirectionOrDefault() == Direction.Forward
                            ? StartScale
                            : EndScale;
            ((RectTransform) targetContainer.transform).localScale = Vector3.one * scale;
        }

        public override ICompletable Perform(GameObject sourceContainer,
                                             GameObject targetContainer,
                                             IOptions options,
                                             float duration)
        {
            return Parallel.Create(
                step =>
                {
                    base.Perform(sourceContainer, targetContainer, options, duration)
                        .In(step);

                    if (sourceContainer != null)
                    {
                        var scale = options.GetDirectionOrDefault() == Direction.Forward
                                        ? EndScale
                                        : StartScale;
                        var rectTransform = (RectTransform) sourceContainer.transform;
                        rectTransform.ScaleTo(scale, duration, Easer)
                                     .In(step);
                    }

                    var targetRectTransform = (RectTransform) targetContainer.transform;
                    targetRectTransform.ScaleTo(1f, duration, Easer)
                                       .In(step);
                });
        }

        public override void Complete(GameObject sourceContainer, GameObject targetContainer)
        {
            base.Complete(sourceContainer, targetContainer);

            if (sourceContainer != null)
                ((RectTransform) sourceContainer.transform).localScale = Vector3.one;

            ((RectTransform) targetContainer.transform).localScale = Vector3.one;
        }
    }
}