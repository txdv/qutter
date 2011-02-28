require 'nokogiri'

FILENAME = "serialization.txt";

def generate(file)

filebasename = File.basename(file, ".xml")

code = Nokogiri::XML(open(file))

File.open(filebasename + "_write.c", "w") do |f|
  f.puts "#include <QFile>"
  f.puts "#include <QDebug>"
  f.puts code.children.at("cinclude").text
  f.puts
  f.puts "int main()\n{"
  f.puts "  QFile file(\"#{FILENAME}\");"
  f.puts "  file.open(QIODevice::WriteOnly);"
  f.puts "  QDataStream out(&file);"
  f.puts code.children.at("cwrite").text
  f.puts "\n  return 0;\n}\n";
end

File.open(filebasename + "_read.c", "w") do |f|
  f.puts "#include <QFile>"
  f.puts "#include <QDebug>"
  f.puts "#include <assert.h>"

  f.puts code.children.at("cinclude").text
  f.puts
  f.puts "int main()\n{"
  f.puts "  QFile file(\"#{FILENAME}\");"
  f.puts "  file.open(QIODevice::ReadOnly);"
  f.puts "  QDataStream in(&file);";

  f.puts code.children.at("cread").text
  f.puts "\n  return 0;\n}\n";
end

File.open(filebasename + "_write.cs", "w") do |f|
  f.puts "#define DEBUG"
  f.puts "using System;"
  f.puts "using System.IO;"
  f.puts "using System.Collections.Generic;"
  f.puts "using Qutter;"
  f.puts "public class MainClass"
  f.puts "{";
  f.puts "  public static void Main(string[] args)"
  f.puts "  {"
  f.puts "    FileStream fs = File.OpenWrite(\"#{FILENAME}\");"
  f.puts code.children.at("cswrite").text
  f.puts "    fs.Close();"
  f.puts "  }"
  f.puts "}";
end

File.open(filebasename + "_read.cs", "w") do |f|
  f.puts "#define DEBUG"
  f.puts "using System;"
  f.puts "using System.IO;"
  f.puts "using System.Collections.Generic;"
  f.puts "using Qutter;"
  f.puts "using System.Diagnostics;"
  f.puts "public class MainClass"
  f.puts "{";
  f.puts "  public static void Main(string[] args)"
  f.puts "  {"
  f.puts "    FileStream fs = File.OpenRead(\"#{FILENAME}\");"
  f.puts "    Trace.Listeners.Add(new ConsoleTraceListener());"
  f.puts code.children.at("csread").text
  f.puts "    fs.Close();"
  f.puts "  }"
  f.puts "}";
end


File.open("test", "w+") do |f|
  f.puts(filebasename + "_read")
  f.puts(filebasename + "_write")
  f.puts("mono " + filebasename + "_read.exe")
  f.puts("mono " + filebasename + "_write.exe")
end

end

Dir["*.xml"].each do |file|
  puts "Generating for #{file}"
  generate(file)
end

File.delete("test")
puts "Generating test"
File.open("test", "w+") do |test|
Dir["*.xml"].each do |file|
  filebasename = File.basename(file, ".xml")
  puts "Compiling #{filebasename} test suits"
  `gmcs -r:Qutter.dll -debug #{filebasename}_write.cs`
  `gmcs -r:Qutter.dll -debug #{filebasename}_read.cs`
  `g++ \`pkg-config QtCore --libs --cflags\` #{filebasename}_write.c -o #{filebasename}_write`
  `g++ \`pkg-config QtCore --libs --cflags\` #{filebasename}_read.c -o #{filebasename}_read`
  test.puts "mono --debug #{filebasename}_write.exe"
  test.puts "./#{filebasename}_read"
  test.puts "./#{filebasename}_write"
  test.puts "mono --debug #{filebasename}_read.exe"
  test.puts
end
end
