/*
	TUIO C# Library - part of the reacTIVision project
	http://reactivision.sourceforge.net/

	Copyright (c) 2005-2009 Martin Kaltenbrunner <mkalten@iua.upf.edu>

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;

namespace TUIO
{

/**
 * The TuioListener interface provides a simple callback infrastructure which is used by the {@link TuioClient} class
 * to dispatch TUIO events to all registered instances of classes that implement the TuioListener interface defined here.<P>
 * Any class that implements the TuioListener interface is required to implement all of the callback methods defined here.
 * The {@link TuioClient} makes use of these interface methods in order to dispatch TUIO events to all registered TuioListener implementations.<P>
 * <code>
 * public class MyTuioListener implements TuioListener<br/>
 * ...</code><p><code>
 * MyTuioListener listener = new MyTuioListener();<br/>
 * TuioClient client = new TuioClient();<br/>
 * client.addTuioListener(listener);<br/>
 * client.start();<br/>
 * </code>
 *
 * @author Martin Kaltenbrunner
 * @version 1.4
 */
	public interface ITuioListener
	{
		/**
		 * This callback method is invoked by the TuioClient when a new TuioObject is added to the session.
		 *
		 * @param  tobj  the TuioObject reference associated to the addTuioObject event
		 */
		void AddTuioObject(TuioObject tobj);

		/**
		 * This callback method is invoked by the TuioClient when an existing TuioObject is updated during the session.
		 *
		 * @param  tobj  the TuioObject reference associated to the updateTuioObject event
		 */
		void UpdateTuioObject(TuioObject tobj);

		/**
		 * This callback method is invoked by the TuioClient when an existing TuioObject is removed from the session.
		 *
		 * @param  tobj  the TuioObject reference associated to the removeTuioObject event
		 */
		void RemoveTuioObject(TuioObject tobj);

		/**
		 * This callback method is invoked by the TuioClient when a new TuioCursor is added to the session.
		 *
		 * @param  tcur  the TuioCursor reference associated to the addTuioCursor event
		 */
		void AddTuioCursor(TuioCursor tcur);

		/**
		 * This callback method is invoked by the TuioClient when an existing TuioCursor is updated during the session.
		 *
		 * @param  tcur  the TuioCursor reference associated to the updateTuioCursor event
		 */
		void UpdateTuioCursor(TuioCursor tcur);

		/**
		 * This callback method is invoked by the TuioClient when an existing TuioCursor is removed from the session.
		 *
		 * @param  tcur  the TuioCursor reference associated to the removeTuioCursor event
		 */
		void RemoveTuioCursor(TuioCursor tcur);

		/**
		 * This callback method is invoked by the TuioClient to mark the end of a received TUIO message bundle.
		 *
		 * @param  ftime  the TuioTime associated to the current TUIO message bundle
		 */
		void Refresh(TuioTime ftime);
	}
}
