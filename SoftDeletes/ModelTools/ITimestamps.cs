using System;

namespace SoftDeletes.ModelTools
{
    public interface ITimestamps
    {
        /// <summary>
        /// To save the entity create date and time.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// To save the entity update date and time.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}