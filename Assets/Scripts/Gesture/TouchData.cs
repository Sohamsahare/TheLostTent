using UnityEngine;
namespace TheLostTent
{
    public struct TouchData
    {
        public Vector2 currentPosition;
        public Vector2 lastPosition;
        public Vector2 startPosition;
        public Vector2 direction
        {
            get
            {
                return (lastPosition - startPosition);
            }
        }

        public Vector2 normalizeDirection
        {
            get
            {
                return direction.normalized;
            }
        }

    }
}