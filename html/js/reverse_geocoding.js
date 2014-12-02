function create_map(var x, var y) {
	var latlng = new google.maps.LatLng(y, x);
	var options = {
		zoom : 15,
		center : latlng,
		mapTyepId : google.maps.MapTyepId.ROADMAP
	};
	var map = new google.maps.Map(document.getElementById("map_canvas"), options);
}