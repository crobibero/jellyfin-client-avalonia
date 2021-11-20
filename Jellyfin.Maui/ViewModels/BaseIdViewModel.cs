﻿using System;

namespace Jellyfin.Maui.ViewModels
{
    /// <summary>
    /// Base viewmodel that provides an ID property.
    /// </summary>
    public class BaseIdViewModel : BaseViewModel, IInitializeId
    {
        private Guid _id;

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// Initialize the view model's id.
        /// </summary>
        /// <param name="id">The id.</param>
        public void Initialize(Guid id) => Id = id;
    }
}
