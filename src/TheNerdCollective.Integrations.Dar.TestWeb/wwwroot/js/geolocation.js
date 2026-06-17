export function getCurrentPosition() {
    if (!navigator.geolocation) {
        throw new Error("Geolocation understøttes ikke i denne browser.");
    }

    return new Promise((resolve, reject) => {
        navigator.geolocation.getCurrentPosition(
            (position) => resolve({
                latitude: position.coords.latitude,
                longitude: position.coords.longitude,
                accuracy: position.coords.accuracy
            }),
            (error) => reject(new Error(error.message || "Kunne ikke hente position.")),
            { enableHighAccuracy: true, timeout: 15000, maximumAge: 60000 }
        );
    });
}
