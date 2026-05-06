terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  # The region is passed via environment variables (eu-north-1)
}

# Fetch the latest Ubuntu 22.04 AMI (Amazon Machine Image) dynamically, and save it to 'aws_ami_ubuntu' variable.
data "aws_ami" "ubuntu" {
  most_recent = true
  owners      = ["099720109477"] # Canonical official account ID

  filter {
    name   = "name"
    values = ["ubuntu/images/hvm-ssd/ubuntu-jammy-22.04-amd64-server-*"]
  }
}

# Upload the public SSH key to AWS
resource "aws_key_pair" "deployer" {
  key_name   = "telemetry-ssh-key"
  public_key = file("telemetry_key.pub") # Refers to the key you just generated
}

# Create a Security Group (Firewall rules)
resource "aws_security_group" "telemetry_sg" {
  name        = "telemetry-security-group"
  description = "Allow inbound traffic for SSH, API, Grafana, and Prometheus"

  ingress {
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"] # Warning: In production, limit this to your IP!
  }

  ingress {  # Allowing access to the API on port 8080. If we add more services, we should not allow this port to the world.
    description = "Minimal API"
    from_port   = 8080
    to_port     = 8080
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "Grafana"
    from_port   = 3000
    to_port     = 3000
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "Prometheus"
    from_port   = 9090
    to_port     = 9090
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    description = "Allow all outbound traffic"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# Provision the EC2 Instance
resource "aws_instance" "app_server" {
  ami           = data.aws_ami.ubuntu.id  # we read this from the data source we defined earlier
  instance_type = "t3.micro" # t3.micro is the Free Tier eligible instance in eu-north-1
  key_name      = aws_key_pair.deployer.key_name  # our SSH key for access
  vpc_security_group_ids = [aws_security_group.telemetry_sg.id] # attach firewall rules

  tags = {
    Name = "Telemetry-Server"
  }
}

# 5. Output the Public IP of the newly created server
output "server_public_ip" {
  description = "The public IP address of the EC2 instance"
  value       = aws_instance.app_server.public_ip
}