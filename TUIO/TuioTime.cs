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

namespace TUIO
{

/**
 * The TuioTime class is a simple structure that is used to reprent the time that has elapsed since the session start.
 * The time is internally represented as seconds and fractions of microseconds which should be more than sufficient for gesture related timing requirements.
 * Therefore at the beginning of a typical TUIO session the static method initSession() will set the reference time for the session.
 * Another important static method getSessionTime will return a TuioTime object representing the time elapsed since the session start.
 * The class also provides various addtional convience method, which allow some simple time arithmetics.
 *
 * @author Martin Kaltenbrunner
 * @version 1.4
 */
public class TuioTime {

	/**
	 * the time since session start in seconds
	 */
	private long seconds = 0;
	/**
	 * time fraction in microseconds
	 */
	private long micro_seconds = 0;
	/**
	 * the session start time in seconds
	 */
	private static long start_seconds = 0;
	/**
	 * start time fraction in microseconds
	 */
	private static long start_micro_seconds = 0;

	/**
	 * The default constructor takes no arguments and sets
	 * the Seconds and Microseconds attributes of the newly created TuioTime both to zero.
	 */
	public TuioTime () {
		this.seconds = 0;
		this.micro_seconds = 0;
	}

	/**
	 * This constructor takes the provided time represented in total Milliseconds
	 * and assigs this value to the newly created TuioTime.
	 *
	 * @param msec the total time in Millseconds
	 */
	public TuioTime (long msec) {
		this.seconds = msec/1000;
		this.micro_seconds = 1000*(msec%1000);
	}

	/**
	 * This constructor takes the provided time represented in Seconds and Microseconds
	 * and assigs these value to the newly created TuioTime.
	 *
	 * @param sec the total time in seconds
	 * @param usec	the microseconds time component
	 */
	public TuioTime (long sec, long usec) {
		this.seconds = sec;
		this.micro_seconds = usec;
	}

	/**
	 * This constructor takes the provided TuioTime
	 * and assigs its Seconds and Microseconds values to the newly created TuioTime.
	 *
	 * @param ttime the TuioTime used to copy
	 */
	public TuioTime (TuioTime ttime) {
		this.seconds = ttime.getSeconds();
		this.micro_seconds = ttime.getMicroseconds();
	}

	/**
	 * Sums the provided time value represented in total Microseconds to the base TuioTime.
	 *
	 * @param btime	the base TuioTime
	 * @param us	the total time to add in Microseconds
	 * @return the sum of this TuioTime with the provided argument in microseconds
	*/
	public static TuioTime operator + (TuioTime atime, long us) {
		long sec = atime.getSeconds() + us/1000000;
		long usec = atime.getMicroseconds() + us%1000000;
		return new TuioTime(sec,usec);
	}

	/**
	 * Sums the provided TuioTime to the base TuioTime.
	 *
	 * @param btime	the base TuioTime
	 * @param ttime	the TuioTime to add
	 * @return the sum of this TuioTime with the provided TuioTime argument
	 */
	public static TuioTime operator + (TuioTime btime, TuioTime ttime) {
		long sec = btime.getSeconds() + ttime.getSeconds();
		long usec = btime.getMicroseconds() + ttime.getMicroseconds();
		sec += usec/1000000;
		usec = usec%1000000;
		return new TuioTime(sec,usec);
	}

	/**
	 * Subtracts the provided time represented in Microseconds from the base TuioTime.
	 *
	 * @param btime	the base TuioTime
	 * @param us	the total time to subtract in Microseconds
	 * @return the subtraction result of this TuioTime minus the provided time in Microseconds
	 */
	public static TuioTime operator - (TuioTime btime, long us) {
		long sec = btime.getSeconds() - us/1000000;
		long usec = btime.getMicroseconds() - us%1000000;

		if (usec<0) {
			usec += 1000000;
			sec--;
		}

		return new TuioTime(sec,usec);
	}

	/**
	 * Subtracts the provided TuioTime from the private Seconds and Microseconds attributes.
	 *
	 * @param btime	the base TuioTime
	 * @param ttime	the TuioTime to subtract
	 * @return the subtraction result of this TuioTime minus the provided TuioTime
	 */
	public static TuioTime operator - (TuioTime btime, TuioTime ttime) {
		long sec = btime.getSeconds() - ttime.getSeconds();
		long usec = btime.getMicroseconds() - ttime.getMicroseconds();

		if (usec<0) {
			usec += 1000000;
			sec--;
		}

		return new TuioTime(sec,usec);
	}

	/**
	 * Takes a TuioTime argument and compares the provided TuioTime to the private Seconds and Microseconds attributes.
	 *
	 * @param ttime	the TuioTime to compare
	 * @return true if the two TuioTime have equal Seconds and Microseconds attributes
	 */
	public bool Equals(TuioTime ttime) {
		if ((seconds==ttime.getSeconds()) && (micro_seconds==ttime.getMicroseconds())) return true;
		else return false;
	}

	/**
	 * Resets the seconds and micro_seconds attributes to zero.
	 */
	public void reset() {
		seconds = 0;
		micro_seconds = 0;
	}

	/**
	 * Returns the TuioTime Seconds component.
	 * @return the TuioTime Seconds component
	 */
	public long getSeconds() {
		return seconds;
	}

	/**
	 * Returns the TuioTime Microseconds component.
	 * @return the TuioTime Microseconds component
	 */
	public long getMicroseconds() {
		return micro_seconds;
	}

	/**
	 * Returns the total TuioTime in Milliseconds.
	 * @return the total TuioTime in Milliseconds
	 */
	public long getTotalMilliseconds() {
		return seconds*1000+micro_seconds/1000;
	}

	/**
	 * This static method globally resets the TUIO session time.
	 */
	public static void initSession() {
		TuioTime startTime = getSystemTime();
		start_seconds = startTime.getSeconds();
		start_micro_seconds = startTime.getMicroseconds();
	}

	/**
	 * Returns the present TuioTime representing the time since session start.
	 * @return the present TuioTime representing the time since session start
	 */
	public static TuioTime getSessionTime() {
		return getSystemTime()-getStartTime();
	}

	/**
	 * Returns the absolut TuioTime representing the session start.
	 * @return the absolut TuioTime representing the session start
	 */
	public static TuioTime getStartTime() {
		return new TuioTime(start_seconds,start_micro_seconds);
	}

	/**
	 * Returns the absolut TuioTime representing the current system time.
	 * @return the absolut TuioTime representing the current system time
	 */
	public static TuioTime getSystemTime() {
		long usec = DateTime.UtcNow.Ticks/10;
		return new TuioTime(usec/1000000,usec%1000000);
	}
}
}