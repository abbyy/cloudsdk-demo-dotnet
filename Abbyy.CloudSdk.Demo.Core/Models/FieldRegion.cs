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

namespace Abbyy.CloudSdk.Demo.Core.Models
{
	public class FieldRegion
	{
		public FieldRegion(int left, int top, int right, int bottom)
		{
			_left = left;
			_top = top;
			_right = right;
			_bottom = bottom;
		}

		public string Format()
		{
			string formatted = $"{_left},{_top},{_right},{_bottom}";
			return formatted;
		}

		private readonly int _left;
		private readonly int _top;
		private readonly int _right;
		private readonly int _bottom;
	}
}
