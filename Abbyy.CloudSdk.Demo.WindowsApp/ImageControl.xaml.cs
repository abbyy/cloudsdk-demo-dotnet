// Copyright © 2019 ABBYY Production LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Abbyy.CloudSdk.Sample.WindowsApp;

namespace Abbyy.CloudSdk.Demo.WindowsApp
{
	public partial class ImageControl : UserControl
	{
		public event EventHandler<RegionSelectedEventArgs> RegionSelected;

		private string _sourceFile;
		private Point _imageCaptureStart;

		public ImageControl()
		{
			InitializeComponent();
			selectBox.Visibility = Visibility.Hidden;
		}

		public string Source
		{
			set
			{
				image.Source = new BitmapImage(new Uri(value));
				_sourceFile = value;
			}
		}

		private void OnRegionSelected(RegionSelectedEventArgs e)
		{
			RegionSelected?.Invoke(this, e);
		}

		private bool HasImage() => image.Source != null;

		private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			image.ReleaseMouseCapture();

			if (selectBox.Visibility != Visibility.Visible) return;

			var imageOffset = VisualTreeHelper.GetOffset(image);
			var visualLeft = Canvas.GetLeft(selectBox);
			var visualTop = Canvas.GetTop(selectBox);

			var width = selectBox.Width;
			var height = selectBox.Height;

			var bmpSource = (BitmapSource)image.Source;

			var scaleX = bmpSource.PixelWidth / image.ActualWidth;
			var scaleY = bmpSource.PixelHeight / image.ActualHeight;

			var newWidth = width * scaleX;
			var newHeight = height * scaleY;

			if ((int)newWidth == 0 || (int)newHeight == 0) return;

			var newX = (visualLeft - imageOffset.X) * scaleX;
			var newY = (visualTop - imageOffset.Y) * scaleY;

			var src = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(_sourceFile);
			var target = new System.Drawing.Bitmap((int)newWidth, (int)newHeight);
			target.SetResolution(src.HorizontalResolution, src.VerticalResolution);

			using (var g = System.Drawing.Graphics.FromImage(target))
			{
				var rect = new System.Drawing.Rectangle((int)newX, (int)newY, (int)newWidth, (int)newHeight);
				g.DrawImage(
					src,
					new System.Drawing.Rectangle(0, 0, target.Width, target.Height),
					rect,
					System.Drawing.GraphicsUnit.Pixel);
			}

			var ev = new RegionSelectedEventArgs(new Rect(newX, newY, newWidth, newHeight), target);
			OnRegionSelected(ev);

			selectBox.Visibility = Visibility.Hidden;
		}

		private void Image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (!HasImage() || e.ClickCount == 2) return;

			_imageCaptureStart = e.GetPosition(canvas);
			image.CaptureMouse();
		}

		private void Image_MouseMove(object sender, MouseEventArgs e)
		{
			if (!HasImage() || !image.IsMouseCaptured) return;

			var imageOffset = VisualTreeHelper.GetOffset(image);
			var currentPosition = e.GetPosition(canvas);

			if (currentPosition.X < imageOffset.X)
			{
				currentPosition.X = imageOffset.X;
			}

			if (currentPosition.Y < imageOffset.Y)
			{
				currentPosition.Y = imageOffset.Y;
			}

			if (currentPosition.X - imageOffset.X > image.ActualWidth)
			{
				currentPosition.X = imageOffset.X + image.ActualWidth;
			}

			if (currentPosition.Y - imageOffset.Y > image.ActualHeight)
			{
				currentPosition.Y = imageOffset.Y + image.ActualHeight;
			}

			selectBox.Visibility = Visibility.Visible;
			selectBox.Stroke = new SolidColorBrush(Colors.Gray);

			selectBox.Width = Math.Abs(currentPosition.X - _imageCaptureStart.X);
			selectBox.Height = Math.Abs(currentPosition.Y - _imageCaptureStart.Y);

			Canvas.SetLeft(selectBox, Math.Min(_imageCaptureStart.X, currentPosition.X));
			Canvas.SetTop(selectBox, Math.Min(_imageCaptureStart.Y, currentPosition.Y));
		}
	}
}
