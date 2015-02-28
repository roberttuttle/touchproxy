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

Overview
--------------------

![](https://github.com/frog-opensource/touchproxy/wiki/images/touchproxy-wiki-1.png)

1. Toggle TouchProxy service on/off (TUIO client / OSC receiver UDP socket listener) 
2. Set TUIO client / OSC receiver UDP socket listener port value (3333 is standard TUIO port) 
3. Select primary or virtual (all active screens) display target for touch injection 
4. Toggle direct touch contact injection into Windows (disabling provides touch hover injection only) 
5. Toggle and set number of simultaneous contacts that will emulate a Windows key/hardware button press 
6. Toggle option to visually display active contact points on top of Windows UI 
7. Toggle Windows hosted Wi-Fi network service on/off 
8. Set SSID value to be used for hosted network (1 to 32 chars, ASCII values only) 
9. Set security key/passphrase (WPA2-Personal) to be used for hosted network (8 to 32 chars, ASCII values only) 
10. Toggle option to save current hosted network key between sessions 
11. Hover to view current hosted network configuration and status info 
12. Calibration sliders for adjusting display edge buffers to compensate for variable physical boundaries in remote TUIO tracker hardware and software. For example, iOS, Android, and Windows Phone devices all report different starting X/Y coordinates at the digitizer display boundaries and do not all necessarily start from (0,0). 
13. Active touch points displayed for both calibration and debug purposes 
14. Current screen resolution dimensions for the selected display target 
15. Current IP addresses for available networks on the TouchProxy host machine that can be used for remote TUIO tracker app configuration (assuming no firewall restrictions or routing issues for desired network and/or port) 
16. Toggle option to display protocol trace output for inbound messages from remote clients 
17. Select protocol message format (OSC or TUIO) to view in trace output display (OSC is lower level than TUIO) 
18. Clear current trace output display

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
