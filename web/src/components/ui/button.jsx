export default function Button({children, className = '', ...props}) {
  return (
    <button
      {...props}
      className={
        'px-4 py-2 rounded-md font-semibold bg-gradient-to-r from-pink-500 via-red-500 to-yellow-500 hover:opacity-90 transition-colors ' +
        className
      }
    >
      {children}
    </button>
  )
}
