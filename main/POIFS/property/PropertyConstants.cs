﻿/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for Additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */

/* ================================================================
 * About NPOI
 * Author: Tony Qu 
 * Author's email: tonyqus (at) gmail.com 
 * Author's Blog: tonyqus.wordpress.com.cn (wp.tonyqus.cn)
 * HomePage: http://www.codeplex.com/npoi
 * Contributors:
 * 
 * ==============================================================*/

namespace NPOI.POIFS.Properties
{
    /// <summary>
    /// Constants used by Properties namespace
    /// </summary>
    public class PropertyConstants
    {
        public const int  PROPERTY_TYPE_OFFSET = 0x42;

        // the property types
        public const byte INVALID_TYPE = 0;
        public const byte DIRECTORY_TYPE       = 1; //STGTY_STORAGE
        public const byte DOCUMENT_TYPE        = 2; //STGTY_STREAM
        public const byte LOCKBYTES_TYPE = 3;       //STGTY_LOCKBYTES
        public const byte PROPERT_TYPE = 4;         //STGTY_PROPERTY
        public const byte ROOT_TYPE = 5;            //STGTY_ROOT
    }
}
