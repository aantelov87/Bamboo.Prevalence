// Bamboo.Prevalence - a .NET object prevalence engine
// Copyright (C) 2002 Rodrigo B. de Oliveira
//
// Based on the original concept and implementation of Prevayler (TM)
// by Klaus Wuestefeld. Visit http://www.prevayler.org for details.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// As a special exception, if you link this library with other files to
// produce an executable, this library does not by itself cause the
// resulting executable to be covered by the GNU General Public License.
// This exception does not however invalidate any other reasons why the
// executable file might be covered by the GNU General Public License.
//
// Contact Information
//
// http://bbooprevalence.sourceforge.net
// mailto:rodrigobamboo@users.sourceforge.net

using System;
using System.IO;
using Nunit.Framework;
using Bamboo.Prevalence;

namespace Bamboo.Prevalence.Tests
{
	/// <summary>
	/// Test case for Bamboo.Prevalence.PrevalenceEngine
	/// </summary>
	[TestFixture]
	public class PrevalenceEngineTest : PrevalenceTestBase
	{	
		private Bamboo.Prevalence.IQuery _queryTotal = new QueryTotal();

		protected override System.Type PrevalentSystemType
		{
			get
			{
				return typeof(AddingSystem);
			}
		}
		
		protected int QueryTotal()
		{
			return (int)ExecuteQuery(_queryTotal);
		}
		
		protected void Add(int amount, int expectedTotal)
		{
			int actualTotal = (int)ExecuteCommand(new AddCommand(amount));
			AssertEquals("Add", expectedTotal, actualTotal);
		}

		protected void Verify(int expectedTotal)
		{
			AssertEquals("System.Total", expectedTotal, QueryTotal());
		}
		
		[SetUp]
		public override void SetUp()
		{	
			base.SetUp();
			ClearPrevalenceBase();
			_engine = CreateEngine();			
		}		

		[Test]
		public void TestPrevalenceEngine()
		{
			Add(10, 10);
			Add(30, 40);
			CrashRecover();
			Verify(40);

			Add(60, 100);
			CrashRecover();
			Verify(100);

			Add(50, 150);
			Snapshot();
			CrashRecover();
			CrashRecover();
			Verify(150);			

			CrashRecover();
			ClearPrevalenceBase();
			Snapshot();
			CrashRecover();
			Verify(150);
			Add(50, 200);

			CrashRecover();
			Verify(200);
		}

		/// <summary>
		/// This test makes sure that is possible
		/// to use the same command twice without
		/// any side effects.
		/// </summary>
		[Test]
		public void TestExecutingTheSameCommandTwice()
		{
			AddCommand command = new AddCommand(10);
			AssertEquals("AddCommand 10", 10, ExecuteCommand(command));

			command.Amount = 20;
			AssertEquals("AddCommand 20", 30, ExecuteCommand(command));

			CrashRecover();
			Verify(30);
		}

		[Test]
		public void TestExceptionThrowingCommand()
		{
			Add(20, 20);

			try
			{
				Add(-10, 0);
				Assert("Add(-10) should throw an exception!", false);
			}
			catch (ArgumentOutOfRangeException)
			{
			}

			CrashRecover();
			Verify(20);
		}

		[Test]
		public void TestRecoverFromEmptySnapshot()
		{
			Snapshot();
			CrashRecover();
			Verify(0);
		}
	}
}
