import { useState } from 'react'
import './App.css'
import Button from './components/ui/button'
import Input from './components/ui/input'

function App() {
  const [latitude, setLatitude] = useState('37.7749')
  const [longitude, setLongitude] = useState('-122.4194')
  const [preferred, setPreferred] = useState('taco')
  const [results, setResults] = useState([])
  const [loading, setLoading] = useState(false)

  async function search() {
    setLoading(true)
    try {
      const query = new URLSearchParams({
        latitude, longitude, count: '5', preferred
      })
      const res = await fetch(`/api/foodtrucks?${query}`)
      const data = await res.json()
      setResults(data)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex flex-col items-center gap-4 p-8 animate-fade-in">
      <h1 className="text-4xl font-bold drop-shadow-lg">Food Truck Finder</h1>
      <div className="flex gap-2">
        <Input value={latitude} onChange={e => setLatitude(e.target.value)} placeholder="Latitude" />
        <Input value={longitude} onChange={e => setLongitude(e.target.value)} placeholder="Longitude" />
        <Input value={preferred} onChange={e => setPreferred(e.target.value)} placeholder="Preferred Food" />
        <Button onClick={search}>Search</Button>
      </div>
      {loading && <p className="animate-pulse">Searching...</p>}
      <ul className="w-full max-w-xl space-y-2">
        {results.map(r => (
          <li key={r.truck.locationId} className="p-4 rounded-lg bg-white/10 backdrop-blur-md">
            <h2 className="text-lg font-semibold">{r.truck.applicant}</h2>
            <p className="text-sm">{r.truck.address}</p>
            <p className="text-sm italic">{r.truck.foodItems}</p>
          </li>
        ))}
      </ul>
    </div>
  )
}

export default App
