using System;
using Android.Graphics;
using Android.Views.Animations;

namespace KenBurnsView
{
	public class Transition {

    /** The rect the transition will start from. */
    private readonly RectF _mSrcRect;

    /** The rect the transition will end at. */
    private readonly RectF _mDstRect;

    /** An intermediary rect that changes in every frame according to the transition progress. */
    private readonly RectF _mCurrentRect = new RectF();

    /** Precomputed width difference between {@link #mSrcRect} and {@link #mSrcRect}. */
    private readonly float _mWidthDiff;
    /** Precomputed height difference between {@link #mSrcRect} and {@link #mSrcRect}. */
    private readonly float _mHeightDiff;
    /** Precomputed X offset between the center points of
     *  {@link #mSrcRect} and {@link #mSrcRect}. */
    private readonly float _mCenterXDiff;
    /** Precomputed Y offset between the center points of
     *  {@link #mSrcRect} and {@link #mSrcRect}. */
    private readonly float _mCenterYDiff;

    /** The duration of the transition in milliseconds. The default duration is 5000 ms. */
    private readonly long _mDuration;

    /** The {@link Interpolator} used to perform the transitions between rects. */
    private readonly IInterpolator _mInterpolator;


    public Transition(RectF srcRect, RectF dstRect, long duration, IInterpolator interpolator) {
        if (!MathUtils.HaveSameAspectRatio(srcRect, dstRect)) {
            throw new IncompatibleRatioException();
        }
        _mSrcRect = srcRect;
        _mDstRect = dstRect;
        _mDuration = duration;
        _mInterpolator = interpolator;

        // Precomputes a few variables to avoid doing it in onDraw().
        _mWidthDiff = dstRect.Width() - srcRect.Width();
        _mHeightDiff = dstRect.Height() - srcRect.Height();
        _mCenterXDiff = dstRect.CenterX() - srcRect.CenterX();
        _mCenterYDiff = dstRect.CenterY() - srcRect.CenterY();
    }


    /**
     * Gets the rect that will take the scene when a Ken Burns transition starts.
     * @return the rect that starts the transition.
     */
    public RectF GetSourceRect() {
        return _mSrcRect;
    }


    /**
     * Gets the rect that will take the scene when a Ken Burns transition ends.
     * @return the rect that ends the transition.
     */
    public RectF GetDestinyRect() {
        return _mDstRect;
    }


    /**
     * Gets the current rect that represents the part of the image to take the scene
     * in the current frame.
     * @param elapsedTime the elapsed time since this transition started.
     */
    public RectF GetInterpolatedRect(long elapsedTime) {
        float elapsedTimeFraction = elapsedTime / (float) _mDuration;
        float interpolationProgress = Math.Min(elapsedTimeFraction, 1);
        float interpolation = _mInterpolator.GetInterpolation(interpolationProgress);
        float currentWidth = _mSrcRect.Width() + (interpolation * _mWidthDiff);
        float currentHeight = _mSrcRect.Height() + (interpolation * _mHeightDiff);

        float currentCenterX = _mSrcRect.CenterX() + (interpolation * _mCenterXDiff);
        float currentCenterY = _mSrcRect.CenterY() + (interpolation * _mCenterYDiff);

        float left = currentCenterX - (currentWidth / 2);
        float top = currentCenterY - (currentHeight / 2);
        float right = left + currentWidth;
        float bottom = top + currentHeight;

        _mCurrentRect.Set(left, top, right, bottom);
        return _mCurrentRect;
    }


    /**
     * Gets the duration of this transition.
     * @return the duration, in milliseconds.
     */
    public long GetDuration() {
        return _mDuration;
    }

}

}