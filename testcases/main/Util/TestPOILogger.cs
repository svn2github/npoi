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

using System;
using System.Text;
using System.Collections.Generic;

using NUnit.Framework;

using NPOI.Util;

namespace TestCases.Util
{
    /// <summary>
    /// Summary description for TestPOILogger
    /// </summary>
    [TestFixture]
    public class TestPOILogger
    {
        public TestPOILogger()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [Test]
        public void TestVariousLogTypes()
        {
            //NKB Testing only that logging classes use gives no exception
            //    Since logging can be disabled, no checking of logging
            //    output is done.

            POILogger log = POILogFactory.GetLogger( "foo" );

            log.Log( POILogger.WARN, "Test = ", 1 );
            log.LogFormatted( POILogger.ERROR, "Test param 1 = %, param 2 = %", "2", 3 );
            log.LogFormatted( POILogger.ERROR, "Test param 1 = %, param 2 = %", new int[]{4, 5} );
            log.LogFormatted( POILogger.ERROR,
                    "Test param 1 = %1.1, param 2 = %0.1", new double[]{4, 5.23} );

        }

    }
}
