using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.ControlsLibrary
{
    /// <summary>
    /// Extension Class to find Child Object
    /// </summary>
    public static class UIElementExtensions
    {
        /// <summary>
        /// Find First Parent Element by Element's Type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="element">Child Element</param>
        /// <returns>Parent Element</returns>
        public static T FindParentOfType<T>(this FrameworkElement element)
        {
            if (element == null)
                return default(T);

            var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;

            while (parent != null)
            {
                if (parent is T)
                    return (T)(object)parent;

                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
            }
            return default(T);
        }

		/// <summary>
		/// Find First Parent Element by Element's Type
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="element">Child Element</param>
		/// <returns>Parent Element</returns>
		public static T FindParentOfType<T>(this UIElement element)
		{
			if (element == null)
				return default(T);

			var parent = VisualTreeHelper.GetParent(element) as UIElement;

			while (parent != null)
			{
				if (parent is T)
					return (T)(object)parent;

				parent = VisualTreeHelper.GetParent(parent) as UIElement;
			}
			return default(T);
		}

        /// <summary>
        /// Find Children Element's Name
        /// </summary>
        /// <typeparam name="FrameworkElement">FrameworkElement</typeparam>
        /// <param name="element">Parent Element</param>
        /// <returns>Child Element</returns>
        public static FrameworkElement GetChildrenByName(this FrameworkElement element, string Name)
        {
            FrameworkElement result = null;

            result = GetChildrenByNameDetail(element, Name, result);

            return result;
        }

        private static FrameworkElement GetChildrenByNameDetail(this FrameworkElement element, string Name, FrameworkElement result)
        {
            if (element == null)
                return null;

            if (result != null)
                return result;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;

                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Button tmpchild = VisualTreeHelper.GetChild(element, i) as Button;
                    if (tmpchild != null)
                    {
                        if (tmpchild.Name == Name)
                        {

                        }
                    }
                }    

                if (child != null)
                {
                    if (child.Name == Name)
                    {
                        result = child;
                        break;
                    }
                    else
                    {
                        result = GetChildrenByNameDetail(child, Name, result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Find Children Element's Type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="element">Parent Element</param>
        /// <returns>List of Child Element</returns>
        public static List<T> GetChildrenByType<T>(this UIElement element) where T : UIElement
        {
            return element.GetChildrenByType<T>(null);
        }

        /// <summary>
        /// Find Children Element's Type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="element">Parent Element</param>
        /// <param name="condition">Condition</param>
        /// <returns>List of Child Element</returns>
        public static List<T> GetChildrenByType<T>(this UIElement element, Func<T, bool> condition) where T : UIElement
        {
            List<T> results = new List<T>();
            GetChildrenByType<T>(element, condition, results);
            return results;
        }

        private static void GetChildrenByType<T>(UIElement element, Func<T, bool> condition, List<T> results) where T : UIElement
        {
            if (element == null)
                return;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                UIElement child = VisualTreeHelper.GetChild(element, i) as UIElement;
                if (child != null)
                {
                    T t = child as T;
                    if (t != null)
                    {
                        if (condition == null)
                        {
                            results.Add(t);
                        }
                        else if (condition(t))
                        {
                            results.Add(t);
                        }
                    }
                    GetChildrenByType<T>(child, condition, results);
                }
            }
        }

        /// <summary>
        /// Whether parent has a child Element or Not
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="element">Parent Element</param>
        /// <param name="condition">Condition</param>
        /// <returns>True/False</returns>
        public static bool HasChildrenByType<T>(this UIElement element, Func<T, bool> condition) where T : UIElement
        {
            return (element.GetChildrenByType<T>(condition).Count != 0);
        }

        public static T GetPrivateField<T>(this object obj, string name)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            FieldInfo field = type.GetField(name, flags);

            if (field == null)
            {
                return default(T);
            }
            else
            {
                return (T)field.GetValue(obj);
            }
        }

        public static void SetPrivateField(this object obj, string name, object value)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            FieldInfo field = type.GetField(name, flags);
            field.SetValue(obj, value);
        }
    }
}
