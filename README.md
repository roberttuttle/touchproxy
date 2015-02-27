TouchProxy
====================

Copyright (c) 2015 frog design inc. / Robert Tuttle <robert.tuttle@frogdesign.com>

A remote touch injection client for Windows 8 using standard TUIO+OSC protocols, variable input calibration, and integrated hosted Wi-Fi networking for devices.

https://github.com/frog-opensource/touchproxy (frog-opensource)

Features
--------------------

- Works with any standard TUIO tracker app or service as a remote multitouch input panel.
http://www.tuio.org/ 
Apps are currently available for: 
  - iOS (https://itunes.apple.com/us/app/tuiopad/id412446962)
  - Android (https://play.google.com/store/apps/details?id=tuioDroid.impl)
  - Many other platforms and environments (http://www.tuio.org/?software)

- Uses the native TouchInjection API for Windows 8 as a proxy for multitouch input hardware 
  - Provides integrated hosted Wi-Fi networking services using the native WlanHostedNetwork API for faster performance between devices or when a local shared network is unavailable 
  - Offers direct and indirect contact modes for both manipulation and/or presentation overlay 
  - Provides detailed calibration of touch inputs to compensate for differences in remote hardware input panels (i.e.: variations in smartphone bezels, digitizer boundaries, etc.) 
  - Allows targeting of primary or virtual display bounds for touch interaction across single or multiple screen resolutions and layouts 
  - Emulates the Windows key and hardware button events using the native Keyboard Input API with configurable touch patterns 
  - Uses updated TUIO and OSC protocol libraries optimized for .NET 4.5 and offers full diagnostic tracing of inbound message streams 
  - See the project wiki for complete options and usage information

License
--------------------
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

References
--------------------
1) TUIO C# Library (with improvements made by Robert Tuttle) 
Copyright (c) 2005-2009 Martin Kaltenbrunner <mkalten@iua.upf.edu>

2) OSC.NET OpenSound Control library for C# (with improvements made by Robert Tuttle) 
