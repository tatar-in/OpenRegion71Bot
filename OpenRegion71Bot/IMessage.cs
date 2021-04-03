using System;
using System.Collections.Generic;
using System.Text;

namespace OpenRegion71Bot
{
    interface IMessage
    {
        /// <summary>
        /// Information about the message 
        /// </summary>
        /// <param name="id">ID of the message</param>
        void InformationAboutMessage(int id);
    }
}
