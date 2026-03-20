import 'package:flutter/material.dart';
import 'package:geolocator/geolocator.dart';
import 'package:audioplayers/audioplayers.dart';
import 'services/api_service.dart';
import 'models/location_model.dart';
import 'screens/login_page.dart';

void main() {
  runApp(MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      home: LoginPage(),
    );
  }
}

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  HomePageState createState() => HomePageState();
}

class HomePageState extends State<HomePage> {
  List<LocationModel> locations = [];
  final player = AudioPlayer();
  LocationModel? currentLocation;

  @override
  void initState() {
    super.initState();
    loadLocations();
    trackPosition();
  }

  void loadLocations() async {
    try {
      var data = await ApiService.fetchLocations();
      setState(() {
        locations = data;
      });
    } catch (e) {
      debugPrint('loadLocations error: $e');
    }
  }

  void trackPosition() async {
  await Geolocator.requestPermission();

  Geolocator.getPositionStream().listen((Position position) {
    if (locations.isEmpty) return;

    checkNearby(position);
  });
  }

  void checkNearby(Position pos) {
    for (var loc in locations) {
      double distance = Geolocator.distanceBetween(
        pos.latitude,
        pos.longitude,
        loc.lat,
        loc.lng,
      );

      if (distance < 500 && currentLocation?.id != loc.id) {
        triggerLocation(loc);
        break;
      }
    }
  }

  void triggerLocation(LocationModel loc) async {
    setState(() {
      currentLocation = loc;
    });

    try {
      await player.stop();
      await player.play(UrlSource(loc.audioUrl)).timeout(
        Duration(seconds: 10),
        onTimeout: () {
          debugPrint('Audio timeout: ${loc.audioUrl}');
        },
      );
    } catch (e) {
      debugPrint('Audio play error: $e');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text("Tour Guide App")),
      body: Center(
        child: currentLocation == null
            ? Text("Dang tim dia diem...")
            : Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(currentLocation!.name,
                      style: TextStyle(fontSize: 24)),
                  SizedBox(height: 10),
                  Text(currentLocation!.description),
                ],
              ),
      ),
    );
  }
}