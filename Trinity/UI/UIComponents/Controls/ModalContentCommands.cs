﻿/*
 * Copyright 2012 Benjamin Gale.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Windows.Input;

namespace Trinity.UI.UIComponents.Controls
{
    /// <summary>
    /// Defines common commands for use with the ModalContentPresenter.
    /// </summary>
    public static class ModalContentCommands
    {
        private static ICommand _showModalContent;
        private static ICommand _hideModalContent;

        /// <summary>
        /// Gets the value that represents the show modal content command.
        /// </summary>
        public static ICommand ShowModalContent
        {
            get
            {
                if (_showModalContent == null) 
                {
                    _showModalContent = new RoutedUICommand("Show Modal Content", "ShowModalContent", typeof(ModalContentCommands));
                }

                return _showModalContent;
            }
        }

        /// <summary>
        /// Gets the value that represents the hide modal content command.
        /// </summary>
        public static ICommand HideModalContent
        {
            get
            {
                if (_hideModalContent == null)
                {
                    _hideModalContent = new RoutedUICommand("Hide Modal Content", "HideModalContent", typeof(ModalContentCommands));
                }

                return _hideModalContent;
            }
        }
    }
}
