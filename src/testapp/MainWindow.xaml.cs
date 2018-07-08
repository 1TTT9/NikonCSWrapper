//
// This work is licensed under a Creative Commons Attribution 3.0 Unported License.
//
// Thomas Dideriksen (thomas@dideriksen.com)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;
using Nikon;

namespace testapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Model model = this.Resources["Model"] as Model;

            if (model != null)
            {
                model.Shutdown();
            }

            base.OnClosing(e);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.ScrollToEnd();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Log.GetInstance().Clear();
        }

        object GetDataContext(object sender)
        {
            FrameworkElement element = sender as FrameworkElement;

            if (element != null)
            {
                return element.DataContext;
            }

            return null;
        }

        private void Button_StartLiveView_Click(object sender, RoutedEventArgs e)
        {
            ObjectModel model = GetDataContext(sender) as ObjectModel;

            if (model != null)
            {
                model.StartLiveView();
            }
        }

        private void Button_StopLiveView_Click(object sender, RoutedEventArgs e)
        {
            ObjectModel model = GetDataContext(sender) as ObjectModel;

            if (model != null)
            {
                model.StopLiveView();
            }
        }

        private void Button_Execute_Click(object sender, RoutedEventArgs e)
        {
            CapModel model = GetDataContext(sender) as CapModel;

            if (model != null)
            {
                model.Start();
            }
        }
    }

    public class ValidatorBase<T> : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string str = value as string;

            if (str == null)
            {
                return new ValidationResult(false, "Value can't be null");
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));

            if (converter != null)
            {
                try
                {
                    T dummy = (T)converter.ConvertFromString(str);
                    return new ValidationResult(true, null);
                }
                catch (Exception)
                {
                    return new ValidationResult(false, "Failed to parse value");
                }
            }
            else
            {
                return new ValidationResult(false, "Failed to parse value");
            }
        }
    }

    public class UnsignedValidator : ValidatorBase<uint> { }
    public class IntegerValidator : ValidatorBase<int> { }
    public class FloatValidator : ValidatorBase<double> { }

    public class ConverterBase<T> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                return value.ToString();
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value as string;
            var converter = TypeDescriptor.GetConverter(typeof(T));

            if (str != null && converter != null)
            {
                try
                {
                    return (T)converter.ConvertFromString(str);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }

    public class UnsignedToString : ConverterBase<uint> { }
    public class IntegerToString : ConverterBase<int> { }
    public class FloatToString : ConverterBase<double> { }

    public class BoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool input = (bool)value;
            return input ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CapTypeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EnumTemplate { get; set; }
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate RangeTemplate { get; set; }
        public DataTemplate GenericTemplate { get; set; }
        public DataTemplate ProcessTemplate { get; set; }
        public DataTemplate BooleanTemplate { get; set; }
        public DataTemplate UnsignedTemplate { get; set; }
        public DataTemplate IntegerTemplate { get; set; }
        public DataTemplate FloatTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            eNkMAIDCapType type = (item as CapModel).CapType;

            switch (type)
            {
                case eNkMAIDCapType.kNkMAIDCapType_Enum:
                    return EnumTemplate;

                case eNkMAIDCapType.kNkMAIDCapType_Range:
                    return RangeTemplate;

                case eNkMAIDCapType.kNkMAIDCapType_String:
                    return StringTemplate;

                case eNkMAIDCapType.kNkMAIDCapType_Process:
                    return ProcessTemplate;

                case eNkMAIDCapType.kNkMAIDCapType_Boolean:
                    return BooleanTemplate;

                case eNkMAIDCapType.kNkMAIDCapType_Unsigned:
                    return UnsignedTemplate;

                case eNkMAIDCapType.kNkMAIDCapType_Integer:
                    return IntegerTemplate;

                case eNkMAIDCapType.kNkMAIDCapType_Float:
                    return FloatTemplate;

                default:
                    return GenericTemplate;
            }
        }
    }
}
