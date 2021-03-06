﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace AutoFollow.UI
{
    [DataContract]
    public class NotifyBase : INotifyPropertyChanged
    {
        private Type _type;

        public NotifyBase()
        {
            SetValuesFromAttributes();
        }

        [IgnoreDataMember]
        public Type DerivedType => _type ?? (_type = GetType());

        protected static double CoerceValue(double value, double min, double max)
        {
            if (value < min)
                value = min;
            else if (value > max)
                value = max;
            return value;
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected string GetLastMethodFullName(int frame = 1)
        {
            var stackTrace = new StackTrace();
            var methodBase = stackTrace.GetFrame(frame).GetMethod();
            if (methodBase.DeclaringType != null)
                return methodBase.DeclaringType.FullName + "." + methodBase.Name + "()";

            return methodBase.Name;
        }

        public void SetValuesFromAttributes()
        {
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this))
            {
                var myAttribute = (DefaultValueAttribute)property.Attributes[typeof(DefaultValueAttribute)];
                if (myAttribute != null)
                {
                    property.SetValue(this, myAttribute.Value);
                }
            }
        }

    }
}






