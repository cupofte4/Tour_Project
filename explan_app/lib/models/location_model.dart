class LocationModel {
  final int id;
  final String name;
  final String description;
  final String audioUrl;
  final double lat;
  final double lng;

  LocationModel({
    required this.id,
    required this.name,
    required this.description,
    required this.audioUrl,
    required this.lat,
    required this.lng,
  });

  factory LocationModel.fromJson(Map<String, dynamic> json) {
    return LocationModel(
      id: json['id'],
      name: json['name'],
      description: json['description'],
      audioUrl: json['audio_url'],
      lat: json['latitude'],
      lng: json['longitude'],
    );
  }
}