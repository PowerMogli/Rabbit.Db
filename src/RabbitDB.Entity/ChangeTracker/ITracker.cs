﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RabbitDB.ChangeTracker
{
    internal interface ITracker : IDisposable
    {
        /// <summary>
        /// Track all properties on the specified object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToTrack"></param>
        void TrackObject<T>(T objectToTrack);

        /// <summary>
        /// Track a specified property on an object or a child object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="objectToTrack"></param>
        /// <param name="trackingExpression"></param>
        void TrackObject<T, TProp>(T objectToTrack, Expression<Func<T, TProp>> trackingExpression);

        /// <summary>
        /// True if any of the objects being tracked is dirty
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Mark all components in the tracker as clean
        /// </summary>
        void MarkAsClean();

        /// <summary>
        /// List of component trackers associated with this tracker
        /// </summary>
        /// <returns></returns>
        IEnumerable<IComponentTracker> ComponentTrackers();

        /// <summary>
        /// Event is raised when the IsDirty flag is changed
        /// </summary>
        event EventHandler<IsDiryChangedArgs> IsDirtyChanged;
    }
}