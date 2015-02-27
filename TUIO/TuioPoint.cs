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
 * The TuioPoint class on the one hand is a simple container and utility class to handle TUIO positions in general,
 * on the other hand the TuioPoint is the base class for the TuioCursor and TuioObject classes.
 *
 * @author Martin Kaltenbrunner
 * @version 1.4
 */
public class TuioPoint {

	/**
	 * X coordinate, representated as a floating point value in a range of 0..1
	 */
	protected float xpos;
	/**
	 * Y coordinate, representated as a floating point value in a range of 0..1
	 */
	protected float ypos;
	/**
	 * The time stamp of the last update represented as TuioTime (time since session start)
	 */
	protected TuioTime currentTime;
	/**
	 * The creation time of this TuioPoint represented as TuioTime (time since session start)
	 */
	protected TuioTime startTime;

	/**
	 * The default constructor takes no arguments and sets
	 * its coordinate attributes to zero and its time stamp to the current session time.
	 */
	public TuioPoint () {
		xpos = 0.0f;
		ypos = 0.0f;
		currentTime = TuioTime.getSessionTime();
		startTime = new TuioTime(currentTime);
	}

	/**
	 * This constructor takes two floating point coordinate arguments and sets
	 * its coordinate attributes to these values and its time stamp to the current session time.
	 *
	 * @param	xp	the X coordinate to assign
	 * @param	yp	the Y coordinate to assign
	 */
	public TuioPoint (float xp, float yp) {
		xpos = xp;
		ypos = yp;
		currentTime = TuioTime.getSessionTime();
		startTime = new TuioTime(currentTime);
	}

	/**
	 * This constructor takes a TuioPoint argument and sets its coordinate attributes
	 * to the coordinates of the provided TuioPoint and its time stamp to the current session time.
	 *
	 * @param	tpoint	the TuioPoint to assign
	 */
	public TuioPoint(TuioPoint tpoint) {
		xpos = tpoint.getX();
		ypos = tpoint.getY();
		currentTime = TuioTime.getSessionTime();
		startTime = new TuioTime(currentTime);
	}

	/**
	 * This constructor takes a TuioTime object and two floating point coordinate arguments and sets
	 * its coordinate attributes to these values and its time stamp to the provided TUIO time object.
	 *
	 * @param	ttime	the TuioTime to assign
	 * @param	xp	the X coordinate to assign
	 * @param	yp	the Y coordinate to assign
	 */
	public TuioPoint(TuioTime ttime, float xp, float yp) {
		xpos = xp;
		ypos = yp;
		currentTime = new TuioTime(ttime);
		startTime = new TuioTime(currentTime);
	}

	/**
	 * Takes a TuioPoint argument and updates its coordinate attributes
	 * to the coordinates of the provided TuioPoint and leaves its time stamp unchanged.
	 *
	 * @param	tpoint	the TuioPoint to assign
	 */
	public void update(TuioPoint tpoint) {
		xpos = tpoint.getX();
		ypos = tpoint.getY();
	}

	/**
	 * Takes two floating point coordinate arguments and updates its coordinate attributes
	 * to the coordinates of the provided TuioPoint and leaves its time stamp unchanged.
	 *
	 * @param	xp	the X coordinate to assign
	 * @param	yp	the Y coordinate to assign
	 */
	public void update(float xp, float yp) {
		xpos = xp;
		ypos = yp;
	}

	/**
	 * Takes a TuioTime object and two floating point coordinate arguments and updates its coordinate attributes
	 * to the coordinates of the provided TuioPoint and its time stamp to the provided TUIO time object.
	 *
	 * @param	ttime	the TuioTime to assign
	 * @param	xp	the X coordinate to assign
	 * @param	yp	the Y coordinate to assign
	 */
	public void update(TuioTime ttime, float xp, float yp) {
		xpos = xp;
		ypos = yp;
		currentTime = new TuioTime(ttime);
	}

	/**
	 * Returns the X coordinate of this TuioPoint.
	 * @return	the X coordinate of this TuioPoint
	 */
	public float getX() {
		return xpos;
	}

	/**
	 * Returns the Y coordinate of this TuioPoint.
	 * @return	the Y coordinate of this TuioPoint
	 */
	public float getY() {
		return ypos;
	}

	/**
	 * Returns the distance to the provided coordinates
	 *
	 * @param	xp	the X coordinate of the distant point
	 * @param	yp	the Y coordinate of the distant point
	 * @return	the distance to the provided coordinates
	 */
	public float getDistance(float x, float y) {
		float dx = xpos-x;
		float dy = ypos-y;
		return (float)Math.Sqrt(dx*dx+dy*dy);
	}

	/**
	 * Returns the distance to the provided TuioPoint
	 *
	 * @param	tpoint	the distant TuioPoint
	 * @return	the distance to the provided TuioPoint
	 */
	public float getDistance(TuioPoint tpoint) {
		return getDistance(tpoint.getX(),tpoint.getY());
	}

	/**
	 * Returns the angle to the provided coordinates
	 *
	 * @param	xp	the X coordinate of the distant point
	 * @param	yp	the Y coordinate of the distant point
	 * @return	the angle to the provided coordinates
	 */
	public float getAngle(float xp, float yp) {

		float side = xp-xpos;
		float height = yp- ypos;
		float distance = getDistance(xp,yp);

		float angle = (float)(Math.Asin(side/distance)+Math.PI/2);
		if (height<0) angle = 2.0f*(float)Math.PI-angle;

		return angle;
	}

	/**
	 * Returns the angle to the provided TuioPoint
	 *
	 * @param	tpoint	the distant TuioPoint
	 * @return	the angle to the provided TuioPoint
	 */
	public float getAngle(TuioPoint tpoint) {
		return getAngle(tpoint.getX(),tpoint.getY());
	}

	/**
	 * Returns the angle in degrees to the provided coordinates
	 *
	 * @param	xp	the X coordinate of the distant point
	 * @param	yp	the Y coordinate of the distant point
	 * @return	the angle in degrees to the provided TuioPoint
	 */
	public float getAngleDegrees(float xp, float yp) {
		return (getAngle(xp,yp)/(float)Math.PI)*180.0f;
	}

	/**
	 * Returns the angle in degrees to the provided TuioPoint
	 *
	 * @param	tpoint	the distant TuioPoint
	 * @return	the angle in degrees to the provided TuioPoint
	 */
	public float getAngleDegrees(TuioPoint tpoint) {
		return (getAngle(tpoint)/(float)Math.PI)*180.0f;
	}

	/**
	 * Returns the X coordinate in pixels relative to the provided screen width.
	 *
	 * @param	width	the screen width
	 * @return	the X coordinate of this TuioPoint in pixels relative to the provided screen width
	 */
	public int getScreenX(int width) {
		return (int)Math.Round(xpos*width);
	}

	/**
	 * Returns the Y coordinate in pixels relative to the provided screen height.
	 *
	 * @param	height	the screen height
	 * @return	the Y coordinate of this TuioPoint in pixels relative to the provided screen height
	 */
	public int getScreenY(int height) {
		return (int)Math.Round(ypos*height);
	}

	/**
	 * Returns the time stamp of this TuioPoint as TuioTime.
	 *
	 * @return	the time stamp of this TuioPoint as TuioTime
	 */
	public TuioTime getTuioTime() {
		return new TuioTime(currentTime);
	}

	/**
	 * Returns the start time of this TuioPoint as TuioTime.
	 *
	 * @return	the start time of this TuioPoint as TuioTime
	 */
	public TuioTime getStartTime() {
		return new TuioTime(startTime);
	}

	}
}
