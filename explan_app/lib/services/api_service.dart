import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/location_model.dart';

class ApiService {
  static const String baseUrl = "http://10.0.2.2:8000";

  static Future<List<LocationModel>> fetchLocations() async {
    final response = await http
    .get(Uri.parse("$baseUrl/locations"))
    .timeout(Duration(seconds: 5));

    if (response.statusCode == 200) {
      List data = jsonDecode(response.body);
      return data.map((e) => LocationModel.fromJson(e)).toList();
      } else {
        throw Exception("Failed to fetch locations: ${response.statusCode} ${response.body}");
    }
  }
}