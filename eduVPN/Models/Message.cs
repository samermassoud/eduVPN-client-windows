﻿/*
    eduVPN - End-user friendly VPN

    Copyright: 2017, The Commons Conservancy eduVPN Programme
    SPDX-License-Identifier: GPL-3.0+
*/

using System;
using Prism.Mvvm;
using System.Collections.Generic;

namespace eduVPN.Models
{
    /// <summary>
    /// eduVPN user/system message base class
    /// </summary>
    public class Message : BindableBase, JSON.ILoadableItem
    {
        #region Properties

        /// <summary>
        /// Message text
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { if (value != _text) { _text = value; RaisePropertyChanged(); } }
        }
        private string _text;

        /// <summary>
        /// Message date and time
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
            set { if (value != _date) { _date = value; RaisePropertyChanged(); } }
        }
        private DateTime _date;

        #endregion

        #region Methods

        public override string ToString()
        {
            return Text;
        }

        #endregion

        #region ILoadableItem Support

        /// <summary>
        /// Loads message from a dictionary object (provided by JSON)
        /// </summary>
        /// <param name="obj">Key/value dictionary with <c>message</c>, <c>date_time</c>, <c>begin</c>, <c>end</c>, and <c>type</c> elements. <c>message</c> and <c>date_time</c> are required. All elements should be strings.</param>
        /// <exception cref="eduJSON.InvalidParameterTypeException"><paramref name="obj"/> type is not <c>Dictionary&lt;string, object&gt;</c></exception>
        public virtual void Load(object obj)
        {
            var obj2 = obj as Dictionary<string, object>;
            if (obj2 == null)
                throw new eduJSON.InvalidParameterTypeException("obj", typeof(Dictionary<string, object>), obj.GetType());

            // Set message text.
            Text = eduJSON.Parser.GetValue<string>(obj2, "message");

            // Set message dates.
            Date = DateTime.Parse(eduJSON.Parser.GetValue<string>(obj2, "date_time"));
        }

        #endregion
    }
}
