﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GUIBase;
namespace TestClient
{

    public class ListItemAttachedBehaviour
    {
        public static DependencyProperty MouseRightClickCommandProperty =
            DependencyProperty.RegisterAttached("MouseRightClickCommand",
                typeof(ICommand),
                typeof(ListItemAttachedBehaviour),
                new FrameworkPropertyMetadata(MousRightClickChanged));
        public static void SetMouseRightClickCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(ListItemAttachedBehaviour.MouseRightClickCommandProperty, value);
        }
        public static ICommand GetMouseRightClickCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(ListItemAttachedBehaviour.MouseRightClickCommandProperty);
        }

        public static DependencyProperty CommandParamProperty =
            DependencyProperty.RegisterAttached("CommandParam",
                typeof(object),
                typeof(ListItemAttachedBehaviour),
                new FrameworkPropertyMetadata(ListItemAttachedBehaviour.CommandParamChanged));
        public static void SetCommandParam(DependencyObject target, object value)
        {
            target.SetValue(ListItemAttachedBehaviour.CommandParamProperty, value);
        }
        public static object GetCommandParam(DependencyObject target)
        {
            return (object)target.GetValue(ListItemAttachedBehaviour.CommandParamProperty);
        }
        private static void CommandParamChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = target as UIElement;
            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    element.SetValue(ListItemAttachedBehaviour.CommandParamProperty, e.NewValue);
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    element.SetValue(ListItemAttachedBehaviour.CommandParamProperty, e.NewValue);
                }

            }
        }

        private static void ItemRightClicked(object sender, MouseEventArgs e)
        {
            UIElement element = sender as UIElement;
            ICommand command = element.GetValue(ListItemAttachedBehaviour.MouseRightClickCommandProperty) as ICommand;
            command.Execute(element.GetValue(ListItemAttachedBehaviour.CommandParamProperty));
            e.Handled = true;
        }
        private static void MousRightClickChanged(DependencyObject target,DependencyPropertyChangedEventArgs e)
        {
            UIElement element = target as UIElement;
            if (element != null)
            {
                if ((e.NewValue != null) && (e.OldValue == null))
                {
                    element.PreviewMouseRightButtonDown += ItemRightClicked;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    element.PreviewMouseRightButtonDown -= ItemRightClicked;
                }
            }
        }
    }

    public class ListItem : ViewModelBase
    {
        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }
        private string _itemName = string.Empty;
        public string ItemName
        {
            get => _itemName;
            set
            {
                if (value != _itemName)
                {
                    _itemName = value;
                    NotifyPropertyChanged("ItemName");
                }
            }
        }
        public bool UseCustomerRightClick { get; set; } = false;
        public Action OwnerCallBack { get; set; } = delegate { };
        private void MouseRightClick(object obj)
        {
            OwnerCallBack();
        }
                
        private RelayCommand _mouseRightClickCommand;
        public ICommand MouseRightClickCommand
        {
            get 
            {
                if (_mouseRightClickCommand == null)
                {
                    _mouseRightClickCommand = new RelayCommand(MouseRightClick, delegate { return true; });
                }
                return _mouseRightClickCommand;
            }
        }
        public ListItem(string itemName = null, bool isSelected = false)
        {
            IsSelected = isSelected;
            ItemName = itemName;
        }
    }

    class SameListItem : EqualityComparer<ListItem>
    {
        public override bool Equals(ListItem x, ListItem y)
        {
            if (x.ItemName == y.ItemName)
                return true;
            return false;
        }

        public override int GetHashCode(ListItem obj)
        {
            return obj.ItemName.Length;
        }
    }
}
