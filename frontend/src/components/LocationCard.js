function LocationCard({ location }) {
  if (!location) return <h2>Đang tìm địa điểm gần bạn...</h2>;

  return (
    <div>
      <h2>{location.name}</h2>

      <img
        src={"http://localhost:5093/images/" + location.image}
        width="300"
        alt=""
      />

      <p>{location.description}</p>

      <audio id="audio" controls />
    </div>
  );
}

export default LocationCard;