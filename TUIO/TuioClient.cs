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
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/

using System;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using OSC.Windows;

namespace TUIO
{
	/**
	 * The TuioClient class is the central TUIO protocol decoder component. It provides a simple callback infrastructure using the {@link TuioListener} interface.
	 * In order to receive and decode TUIO messages an instance of TuioClient needs to be created. The TuioClient instance then generates TUIO events
	 * which are broadcasted to all registered classes that implement the {@link TuioListener} interface.<P>
	 * <code>
	 * TuioClient client = new TuioClient();<br/>
	 * client.addTuioListener(myTuioListener);<br/>
	 * client.start();<br/>
	 * </code>
	 *
	 * @author Martin Kaltenbrunner
	 * @version 1.4
	 */
	// HACK : Implemented IDisposable interface per code analysis (@rktut)
	public class TuioClient : IDisposable
	{
		private bool _isDisposed = false;

		private bool connected = false;
		private int port = 3333;
		private OSCReceiver receiver;
		private Thread thread;

		private object cursorSync = new object();
		private object objectSync = new object();

		private Dictionary<long,TuioObject> objectList = new Dictionary<long,TuioObject>(32);
		private List<long> aliveObjectList = new List<long>(32);
		private List<long> newObjectList = new List<long>(32);
		private Dictionary<long,TuioCursor> cursorList = new Dictionary<long,TuioCursor>(32);
		private List<long> aliveCursorList = new List<long>(32);
		private List<long> newCursorList = new List<long>(32);
		private List<TuioObject> frameObjects = new List<TuioObject>(32);
		private List<TuioCursor> frameCursors = new List<TuioCursor>(32);

		private List<TuioCursor> freeCursorList = new List<TuioCursor>();
		private int maxCursorID = -1;

		private int currentFrame = 0;
		private TuioTime currentTime;

		private List<ITuioListener> listenerList = new List<ITuioListener>();

		/**
		 * The default constructor creates a client that listens to the default TUIO port 3333
		 */
		public TuioClient() {}

		/**
		 * This constructor creates a client that listens to the provided port
		 *
		 * @param port the listening port number
		 */
		public TuioClient(int port) {
			this.port = port;
		}

		~TuioClient()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (!_isDisposed)
			{
				if (isDisposing) 
				{
					disconnect();
					receiver.Dispose();
				}
			}
			_isDisposed = true;
		}

		/**
		 * Returns the port number listening to.
		 *
		 * @return the listening port number
		 */
		public int getPort() {
			return port;
		}

		/**
		 * The TuioClient starts listening to TUIO messages on the configured UDP port
		 * All reveived TUIO messages are decoded and the resulting TUIO events are broadcasted to all registered TuioListeners
		 */
		public void connect() 
		{
			TuioTime.initSession();
			currentTime = new TuioTime();
			currentTime.reset();

			receiver = new OSCReceiver(port);
			thread = new Thread(new ThreadStart(listen));

			// HACK : Set listener thread to background added by frog (@rktut)
			thread.IsBackground = true;

			thread.Start();

			connected = true;

			// HACK : Swallowed exception removed by frog (@rktut)
		}

		/**
		 * The TuioClient stops listening to TUIO messages on the configured UDP port
		 */
		public void disconnect() {
			if (receiver!=null) receiver.Close();
			receiver = null;

			aliveObjectList.Clear();
			aliveCursorList.Clear();
			objectList.Clear();
			cursorList.Clear();
			freeCursorList.Clear();
			frameObjects.Clear();
			frameCursors.Clear();
			
			connected = false;
		}

		/**
		 * Returns true if this TuioClient is currently connected.
		 * @return	true if this TuioClient is currently connected
		 */
		public bool isConnected() { return connected; }

		private void listen() 
		{
			while (connected) 
			{
				try 
				{
					OSCPacket packet = receiver.Receive();
					if (packet != null)
					{
						if (packet.IsBundle())
						{
							List<object> messages = packet.Values;
							for (int i = 0, ic = messages.Count; i < ic; i++)
							{
								processMessage((OSCMessage)messages[i]);
							}
						}
						else
						{
							processMessage((OSCMessage)packet);
						}
					}
				} 
				catch (Exception e) 
				{
					// HACK : Exception notification changed to debug output by frog (@rktut)
					System.Diagnostics.Debug.WriteLine(e.ToString());
				}
			}
		}

		/**
		 * The OSC callback method where all TUIO messages are received and decoded
		 * and where the TUIO event callbacks are dispatched
		 *
		 * @param message	the received OSC message
		 */
		private void processMessage(OSCMessage message) {

			string address = message.Address;
			List<object> args = message.Values;
			string command = (string)args[0];

			// HACK : TUIO message tracing added by frog (@rktut)
			Trace.WriteLine(string.Format("{0} {1}", address, string.Join(" ", args.ToArray())), "OSC");

			if (address == "/tuio/2Dobj") {
				if (command == "set") {

					long s_id = (int)args[1];
					int f_id = (int)args[2];
					float xpos = (float)args[3];
					float ypos = (float)args[4];
					float angle = (float)args[5];
					float xspeed = (float)args[6];
					float yspeed = (float)args[7];
					float rspeed = (float)args[8];
					float maccel = (float)args[9];
					float raccel = (float)args[10];

					lock(objectSync) {
						if (!objectList.ContainsKey(s_id)) {
							TuioObject addObject = new TuioObject(s_id,f_id,xpos,ypos,angle);
							frameObjects.Add(addObject);
						} else {
							TuioObject tobj = objectList[s_id];
							if (tobj==null) return;
							if((tobj.getX()!=xpos) || (tobj.getY()!=ypos) || (tobj.getAngle()!=angle) || (tobj.getXSpeed()!=xspeed) || (tobj.getYSpeed()!=yspeed) || (tobj.getRotationSpeed()!=rspeed) || (tobj.getMotionAccel()!=maccel) || (tobj.getRotationAccel()!=raccel)) {
								
								TuioObject updateObject = new TuioObject(s_id,f_id,xpos,ypos,angle);
								updateObject.update(xpos,ypos,angle,xspeed,yspeed,rspeed,maccel,raccel);
								frameObjects.Add(updateObject);
							}
						}
					}

				} else if (command == "alive") {

					newObjectList.Clear();
					for (int i = 1, ic = args.Count; i < ic; i++) {
						// get the message content
						long s_id = (int)args[i];
						newObjectList.Add(s_id);
						// reduce the object list to the lost objects
						if (aliveObjectList.Contains(s_id))
							 aliveObjectList.Remove(s_id);
					}

					// remove the remaining objects
					lock(objectSync) {
						for (int i = 0, ic = aliveObjectList.Count; i < ic; i++)
						{
							long s_id = aliveObjectList[i];
							TuioObject removeObject = objectList[s_id];
							removeObject.remove(currentTime);
							frameObjects.Add(removeObject);
						}
					}

				} else if (command=="fseq") {
					int fseq = (int)args[1];
					bool lateFrame = false;

					if (fseq>0) {
						if (fseq>currentFrame) currentTime = TuioTime.getSessionTime();
						if ((fseq>=currentFrame) || ((currentFrame-fseq)>100)) currentFrame = fseq;
						else lateFrame = true;
					} else if ((TuioTime.getSessionTime().getTotalMilliseconds()-currentTime.getTotalMilliseconds())>100) {
						currentTime = TuioTime.getSessionTime();
					}

					if (!lateFrame) {

						IEnumerator<TuioObject> frameEnum = frameObjects.GetEnumerator();
						while(frameEnum.MoveNext()) {
							TuioObject tobj = frameEnum.Current;

							switch (tobj.getTuioState()) {
								case TuioObject.TUIO_REMOVED:
									TuioObject removeObject = tobj;
									removeObject.remove(currentTime);

									for (int i = 0, ic = listenerList.Count; i < ic; i++)
									{
										ITuioListener listener = (ITuioListener)listenerList[i];
										if (listener!=null) listener.RemoveTuioObject(removeObject);
									}
									lock(objectSync) {
										objectList.Remove(removeObject.getSessionID());
									}
									break;
								case TuioObject.TUIO_ADDED:
									TuioObject addObject = new TuioObject(currentTime,tobj.getSessionID(),tobj.getSymbolID(),tobj.getX(),tobj.getY(),tobj.getAngle());
									lock(objectSync) {
										objectList.Add(addObject.getSessionID(),addObject);
									}
									for (int i = 0, ic = listenerList.Count; i < ic; i++)
									{
										ITuioListener listener = (ITuioListener)listenerList[i];
										if (listener!=null) listener.AddTuioObject(addObject);
									}
									break;
								default:
									TuioObject updateObject = getTuioObject(tobj.getSessionID());
									if ( (tobj.getX()!=updateObject.getX() && tobj.getXSpeed()==0) || (tobj.getY()!=updateObject.getY() && tobj.getYSpeed()==0) )
										updateObject.update(currentTime,tobj.getX(),tobj.getY(),tobj.getAngle());
									else
										updateObject.update(currentTime,tobj.getX(),tobj.getY(),tobj.getAngle(),tobj.getXSpeed(),tobj.getYSpeed(),tobj.getRotationSpeed(),tobj.getMotionAccel(),tobj.getRotationAccel());

									for (int i = 0, ic = listenerList.Count; i < ic; i++)
									{
										ITuioListener listener = (ITuioListener)listenerList[i];
										if (listener!=null) listener.UpdateTuioObject(updateObject);
									}
									break;
							}
						}

						for (int i = 0, ic = listenerList.Count; i < ic; i++)
						{
							ITuioListener listener = (ITuioListener)listenerList[i];
							if (listener!=null) listener.Refresh(new TuioTime(currentTime));
						}

						List<long> buffer = aliveObjectList;
						aliveObjectList = newObjectList;
						// recycling the List
						newObjectList = buffer;
					}
					frameObjects.Clear();
				}

			} else if (address == "/tuio/2Dcur") {

				if (command == "set") {

					long s_id = (int)args[1];
					float xpos = (float)args[2];
					float ypos = (float)args[3];
					float xspeed = (float)args[4];
					float yspeed = (float)args[5];
					float maccel = (float)args[6];

					lock(cursorList) {
						if (!cursorList.ContainsKey(s_id)) {

							TuioCursor addCursor = new TuioCursor(s_id,-1,xpos,ypos);
							frameCursors.Add(addCursor);

						} else {
							TuioCursor tcur = (TuioCursor)cursorList[s_id];
							if (tcur==null) return;
							if ((tcur.getX()!=xpos) || (tcur.getY()!=ypos) || (tcur.getXSpeed()!=xspeed) || (tcur.getYSpeed()!=yspeed) || (tcur.getMotionAccel()!=maccel)) {
								TuioCursor updateCursor = new TuioCursor(s_id,tcur.getCursorID(),xpos,ypos);
								updateCursor.update(xpos,ypos,xspeed,yspeed,maccel);
								frameCursors.Add(updateCursor);
							}
						}
					}

				} else if (command == "alive") {

					newCursorList.Clear();
					for (int i = 1, ic = args.Count; i < ic; i++)
					{
						// get the message content
						long s_id = (int)args[i];
						newCursorList.Add(s_id);
						// reduce the cursor list to the lost cursors
						if (aliveCursorList.Contains(s_id))
							aliveCursorList.Remove(s_id);
					}

					// remove the remaining cursors
					lock(cursorSync) {
						for (int i = 0, ic = aliveCursorList.Count; i < ic; i++)
						{
							long s_id = aliveCursorList[i];
							if (!cursorList.ContainsKey(s_id)) continue;
							TuioCursor removeCursor = cursorList[s_id];
 							removeCursor.remove(currentTime);
							frameCursors.Add(removeCursor);
						}
					}

				} else if (command=="fseq") {
					int fseq = (int)args[1];
					bool lateFrame = false;

					if (fseq>0) {
						if (fseq>currentFrame) currentTime = TuioTime.getSessionTime();
						if ((fseq>=currentFrame) || ((currentFrame-fseq)>100)) currentFrame = fseq;
						else lateFrame = true;
					} else if ((TuioTime.getSessionTime().getTotalMilliseconds()-currentTime.getTotalMilliseconds())>100) {
						currentTime = TuioTime.getSessionTime();
					}

					if (!lateFrame) {

						IEnumerator<TuioCursor> frameEnum = frameCursors.GetEnumerator();
						while(frameEnum.MoveNext()) {
							TuioCursor tcur = frameEnum.Current;
							switch (tcur.getTuioState()) {
								case TuioCursor.TUIO_REMOVED:
									TuioCursor removeCursor = tcur;
									removeCursor.remove(currentTime);

									for (int i = 0, ic = listenerList.Count; i < ic; i++)
									{
										ITuioListener listener = (ITuioListener)listenerList[i];
										if (listener!=null) listener.RemoveTuioCursor(removeCursor);
									}
									lock(cursorSync) {
										cursorList.Remove(removeCursor.getSessionID());

										if (removeCursor.getCursorID() == maxCursorID) {
											maxCursorID = -1;

											if (cursorList.Count > 0) {

												IEnumerator<KeyValuePair<long, TuioCursor>> clist = cursorList.GetEnumerator();
												while (clist.MoveNext()) {
													int f_id = clist.Current.Value.getCursorID();
													if (f_id > maxCursorID) maxCursorID = f_id;
												}

							 					List<TuioCursor> freeCursorBuffer = new List<TuioCursor>();
							 					IEnumerator<TuioCursor> flist = freeCursorList.GetEnumerator();
												while (flist.MoveNext()) {
								 					TuioCursor testCursor = flist.Current;
													if (testCursor.getCursorID() < maxCursorID) freeCursorBuffer.Add(testCursor);
												}
												freeCursorList = freeCursorBuffer;
											} else freeCursorList.Clear();
										} else if (removeCursor.getCursorID() < maxCursorID) freeCursorList.Add(removeCursor);
									}
									break;

							case TuioCursor.TUIO_ADDED:
								TuioCursor addCursor;
								lock(cursorSync) {
									int c_id = cursorList.Count;
									if ((cursorList.Count<=maxCursorID) && (freeCursorList.Count>0)) {
										TuioCursor closestCursor = freeCursorList[0];
										IEnumerator<TuioCursor> testList = freeCursorList.GetEnumerator();
										while(testList.MoveNext()) {
											TuioCursor testCursor = testList.Current;
											if (testCursor.getDistance(tcur)<closestCursor.getDistance(tcur)) closestCursor = testCursor;
										}
										c_id = closestCursor.getCursorID();
										freeCursorList.Remove(closestCursor);
									} else maxCursorID = c_id;

									addCursor = new TuioCursor(currentTime,tcur.getSessionID(),c_id,tcur.getX(),tcur.getY());
									cursorList.Add(addCursor.getSessionID(),addCursor);
								}

								for (int i = 0, ic = listenerList.Count; i < ic; i++)
								{
									ITuioListener listener = (ITuioListener)listenerList[i];
									if (listener!=null) listener.AddTuioCursor(addCursor);
								}
								break;

							default:
								TuioCursor updateCursor = getTuioCursor(tcur.getSessionID());
								if ( (tcur.getX()!=updateCursor.getX() && tcur.getXSpeed()==0) || (tcur.getY()!=updateCursor.getY() && tcur.getYSpeed()==0) )
									updateCursor.update(currentTime,tcur.getX(),tcur.getY());
								else
									updateCursor.update(currentTime,tcur.getX(),tcur.getY(),tcur.getXSpeed(),tcur.getYSpeed(),tcur.getMotionAccel());

								for (int i = 0, ic = listenerList.Count; i < ic; i++)
								{
									ITuioListener listener = (ITuioListener)listenerList[i];
									if (listener!=null) listener.UpdateTuioCursor(updateCursor);
								}
								break;
							}
						}

						for (int i = 0, ic = listenerList.Count; i < ic; i++)
						{
							ITuioListener listener = (ITuioListener)listenerList[i];
							if (listener!=null) listener.Refresh(new TuioTime(currentTime));
						}

						List<long> buffer = aliveCursorList;
						aliveCursorList = newCursorList;
						// recycling the List
						newCursorList = buffer;
					}
					frameCursors.Clear();
				}

			}
		}

		/**
		 * Adds the provided TuioListener to the list of registered TUIO event listeners
		 *
		 * @param listener the TuioListener to add
		 */
		public void addTuioListener(ITuioListener listener) {
			listenerList.Add(listener);
		}

		/**
		 * Removes the provided TuioListener from the list of registered TUIO event listeners
		 *
		 * @param listener the TuioListener to remove
		 */
		public void removeTuioListener(ITuioListener listener) {
			listenerList.Remove(listener);
		}

		/**
		 * Removes all TuioListener from the list of registered TUIO event listeners
		 */
		public void removeAllTuioListeners() {
			listenerList.Clear();
		}

		/**
		 * Returns a Vector of all currently active TuioObjects
		 *
		 * @return a Vector of all currently active TuioObjects
		 */
		public List<TuioObject> getTuioObjects() {
			List<TuioObject> listBuffer;
			lock(objectSync) {
				listBuffer = new List<TuioObject>(objectList.Values);
			}
			return listBuffer;
		}

		/**
		 * Returns a Vector of all currently active TuioCursors
		 *
		 * @return a Vector of all currently active TuioCursors
		 */
		public List<TuioCursor> getTuioCursors() {
			List<TuioCursor> listBuffer;
			lock(cursorSync) {
				listBuffer = new List<TuioCursor>(cursorList.Values);
			}
			return listBuffer;
		}

		/**
		 * Returns the TuioObject corresponding to the provided Session ID
		 * or NULL if the Session ID does not refer to an active TuioObject
		 *
		 * @return an active TuioObject corresponding to the provided Session ID or NULL
		 */
		public TuioObject getTuioObject(long s_id) {
			TuioObject tobject = null;
			lock(objectSync) {
				objectList.TryGetValue(s_id,out tobject);
			}
			return tobject;
		}

		/**
		 * Returns the TuioCursor corresponding to the provided Session ID
		 * or NULL if the Session ID does not refer to an active TuioCursor
		 *
		 * @return an active TuioCursor corresponding to the provided Session ID or NULL
		 */
		public TuioCursor getTuioCursor(long s_id) {
			TuioCursor tcursor = null;
			lock(cursorSync) {
				cursorList.TryGetValue(s_id, out tcursor);
			}
			return tcursor;
		}

	}
}
