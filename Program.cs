using System;
using System.Numerics;
using System.Collections.Generic;

public delegate Vector2 FdblVector2( double x, double y );


struct DataItem 
{
	public double X { get; set; }
	public double Y { get; set; }
	public System.Numerics.Vector2 E { get; set; }
	public DataItem( double a, double b, System.Numerics.Vector2 c)
	{
		this.X = a;
		this.Y = b;
		this.E = c;
	}   
	public override string ToString()
	{
		return string.Format("{0},{1},{2},{3},{4}"						, X, Y, E.X, E.Y, E.Length());
	} 
	public string ToLongString( string format )
	{
		string str_out = "";
		str_out += String.Format( format, X);
		str_out += String.Format( format, Y);
		str_out += String.Format( format, E.X);
		str_out += String.Format( format, E.Y);
		str_out += String.Format( format, E.Length()) + "\n";
		return str_out;
	}
};

abstract class V3Data
{
	public string name { get; }
	public DateTime dttm { get; }
	public V3Data( string str, DateTime date )
	{
		this.name = str;
		this.dttm = date;
	}
	public abstract int Count { get; }
	public abstract double MaxDistance { get; }
	public abstract string ToLongString( string format );
	public override string ToString()
	{
		return( String.Format("{0],{1}", name, dttm ));
	}
};

class V3DataList : V3Data
{
	public List<DataItem> lst { get; }
	public override double MaxDistance 
	{ 
		get
		{
			double maxd = 0;
			foreach ( DataItem i in lst )
			{
				foreach ( DataItem p in lst )
				{
					double x = i.X - p.X;
					double y = i.Y - p.Y;
					float a = Convert.ToSingle(x);
					float b = Convert.ToSingle(y);
					Vector2 vec = new Vector2(a,b);
					if( vec.Length() > maxd)
					{
						maxd = vec.Length();
					}
				}
			}
			return maxd;
		}
	}
	public override int Count { get { return lst.Count; } }
	public V3DataList( string str, DateTime date ):base(str,date)
	{
		this.lst = new List<DataItem>();
	}
	public bool Add( DataItem newItem )
	{
		foreach ( DataItem i in lst )
		{
			float a = Convert.ToSingle(i.X - newItem.X);
			float b = Convert.ToSingle(i.Y - newItem.Y);
			Vector2 vec = new Vector2( a, b );
			if( vec.Length() == 0)
			{
				return false;
			}
		}
		this.lst.Add( newItem );
		return true;
	}
	public int AddDefaults( int nItems, FdblVector2 F)
	{
		double x = 0, y = 0, dx = 0.1;
		int i, counter = 0;
		x = this.Count*dx;
		for( i = 1; i <= nItems; i++ )
		{
			x = x + i*dx;
			y = x*x/3;
			DataItem nItem = new DataItem( x, y, F(x,y));
			if( Add( nItem ) )
			{
				counter++;
			}
		}
		return counter;
	}
	public override string ToString()
	{
		return String.Format("V3DataLIst,{0},{1},{2},{3}\n",
			base.name, base.dttm, this.Count, this.MaxDistance);
	}
	public override string ToLongString( string format )
	{
		string out_str = this.ToString();
		foreach ( DataItem p in this.lst )
		{
			out_str = out_str + p.ToLongString( format ); 
		}
		return out_str;
	}
};

class V3DataArray: V3Data
{
	public int Ox { get; }
	public int Oy { get; }
	public double dx { get; }
	public double dy { get; }
	public Vector2[,] Arr { get; } 
	public override int Count { get { return Ox*Oy; } }
	public override double MaxDistance 
	{ 
		get
		{
			float a = Convert.ToSingle((Ox-1)*dx);
			float b = Convert.ToSingle((Oy-1)*dy);
			return ( new Vector2(a,b).Length() );
		}
	}
	public V3DataArray( string str, DateTime date ):base(str, date)
	{
		Arr = new Vector2[2,2];
		for( int i = 0; i < 2; i++ )
		{
			for( int n = 0; n < 2; n++ )
			{
				Arr[i,n] = new Vector2();
			}
		}
	}
	public V3DataArray( string str, DateTime dt, int a, int b, double x,
			double y, FdblVector2 F):base( str, dt)
	{
		this.dx = x;
		this.dy = y;
		this.Ox = a;
		this.Oy = b;
		Arr = new Vector2[Ox,Oy];
		for( int i = 0; i < Ox; i++ )
		{
			for( int n = 0; n < Oy; n++ )
			{
				Arr[i,n] = new Vector2();
				Arr[i,n] = F( i*dx, n*dy );
			}
		}
	}
	public override string ToString()
	{
		return String.Format("V3DataArray,{0},{1},{2},{3},{4},{5}\n",
				base.name,base.dttm,this.Ox,
					this.Oy,this.dx,this.dy);
	}
	public override string ToLongString( string format )
	{
		string str_out = "";
		str_out += this.ToString();
		for( int i = 0; i < Ox; i++ )
		{
			for( int n = 0; n < Oy; n++ )
			{
				str_out += String.Format(format,i*dx);
				str_out += String.Format(format,n*dy);
				str_out += String.Format(format,Arr[i,n].X);
				str_out += String.Format(format,Arr[i,n].Y);
				str_out += "\n";
			}
		}
		return str_out;
	}
	public V3DataList ConvertToV3DataList()
	{
		bool b;
		V3DataList p = new V3DataList( base.name, base.dttm );
		for( int i = 0; i < Ox; i++ )
		{
			for( int n = 0; n < Oy; n++ )
			{
				DataItem v = new DataItem(i*dx, n*dy, Arr[i,n]);
				b = p.Add(v);
			}
		}
		return p;
	}
}

class V3MainCollection
{
	private List<V3Data> Lst;
	public V3MainCollection()
	{
		Lst = new List<V3Data>();
	}
	public V3Data this[int index]
	{
		get { return Lst[index]; }
		set { Lst[index] = value; }
	}
	public int Count { get { return Lst.Count; } }
	public bool Contains(string ID)
	{
		foreach( V3Data p in Lst )
		{
			if( p.name == ID )
			{
				return true;
			}
		}
		return false;
	}
	public bool Add( V3Data v3Data )
	{
		if( this.Contains( v3Data.name ) )
		{
			return false;
		}
		Lst.Add( v3Data );
		return true;
	}
	public string ToLongString( string format )
	{
		string str = "";
		foreach( V3Data p in Lst )
		{
			str += p.ToLongString( format );
		}
		return str + "\n";
	}
	public override string ToString()
	{
		string str = "";
		foreach( V3Data p in Lst )
		{
			str += p.ToString();
		}
		return str + "\n";
	}
};

class Program
{
	static void Main(string[] args)
	{
		bool b;
		int i = 4;
		DateTime dt = new DateTime();
		V3DataList qwe;
		FdblVector2 F;
		F = SimpleVec;
		V3DataArray asd = new V3DataArray("ssmb",dt,3,3,0.1,0.1,F);
		V3DataArray Arr2 = new V3DataArray("Nme2",dt,2,2,0.01,0.01,F);
		V3DataList Lst1 = new V3DataList("Pigeon",dt);
		i = Lst1.AddDefaults( 4 , F );
		V3MainCollection Coll = new V3MainCollection();
		Console.WriteLine( asd.ToLongString("{0:c}, "));
		qwe = asd.ConvertToV3DataList();
		Console.WriteLine( qwe.ToString() );
		Console.WriteLine( "qwe.Count = {0}, qwe.MaxDistance = {1}",
				qwe.Count,qwe.MaxDistance);
		Console.WriteLine("asd.Count = {0}, asd.MaxDistance = {1}\n",
				asd.Count, asd.MaxDistance);
		b = Coll.Add(asd);
		b = Coll.Add(Arr2);
		b = Coll.Add(qwe);
		b = Coll.Add(Lst1);
		Console.WriteLine( Coll.ToLongString("{0,12:F4}, "));
		
	}

	static Vector2 SimpleVec( double x, double y )
	{
		return new Vector2( 12, 34);
	}	
}
