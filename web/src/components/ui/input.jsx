export default function Input(props) {
  return <input {...props} className={'px-3 py-2 rounded-md text-black ' + (props.className ?? '')} />
}
