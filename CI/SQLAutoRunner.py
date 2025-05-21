#!/usr/bin/env python3
"""
PostgreSQL SQL Auto Script Runner

This script processes new .DEP files, reads the SQL script paths from them,
and executes those SQL scripts against a PostgreSQL database in the order they appear.

Usage:
    python sql_auto_script.py --dep-dir <dep_directory> --db-host <host> --db-name <database> --db-user <username> [--db-password <password>]

The script keeps track of processed .DEP files to avoid running the same scripts multiple times.
"""

import os
import sys
import argparse
import logging
import psycopg2
from datetime import datetime
from typing import List, Optional, Set

# Configure logging
log_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "CI/logs")
if not os.path.exists(log_dir):
    os.makedirs(log_dir)

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler(os.path.join(log_dir, "SQLAutoRunner.log")),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

class PostgresScriptRunner:
    def __init__(self, 
                 dep_dir: str,
                 db_host: str, 
                 db_name: str, 
                 db_user: str, 
                 db_password: Optional[str] = None,
                 db_port: int = 5432):
        """
        Initialize the PostgreSQL Script Runner.
        
        Args:
            dep_dir: Directory to monitor for .DEP files
            db_host: Database host
            db_name: Database name
            db_user: Database username
            db_password: Database password
            db_port: Database port (default: 5432)
        """
        self.dep_dir = dep_dir
        self.db_host = db_host
        self.db_name = db_name
        self.db_user = db_user
        self.db_password = db_password
        self.db_port = db_port
            
        # State file to track processed .DEP files
        state_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "logs")
        if not os.path.exists(state_dir):
            os.makedirs(state_dir)
            
        self.state_file = os.path.join(state_dir, "processed_deps.txt")
        self.processed_deps = self._load_processed_deps()
    
    def _load_processed_deps(self) -> Set[str]:
        """Load the set of already processed .DEP files."""
        processed = set()
        if os.path.exists(self.state_file):
            with open(self.state_file, 'r') as f:
                for line in f:
                    processed.add(line.strip())
        return processed
    
    def _save_processed_dep(self, dep_file: str) -> None:
        """Save a processed .DEP file to the state file."""
        with open(self.state_file, 'a') as f:
            f.write(dep_file + '\n')
        self.processed_deps.add(dep_file)
    
    def _connect_to_db(self):
        """Establish a connection to the PostgreSQL database."""
        try:
            # Build connection string with optional parameters
            dsn_parts = [
                f"host={self.db_host}",
                f"port={self.db_port}",
                f"dbname={self.db_name}",
                f"user={self.db_user}"
            ]
            
            if self.db_password:
                dsn_parts.append(f"password={self.db_password}")
                
            # Add additional PostgreSQL connection parameters
            dsn_parts.append("application_name=sql_auto_script")
            dsn_parts.append("connect_timeout=10")
            
            dsn = " ".join(dsn_parts)
            
            connection = psycopg2.connect(dsn)
            # Set autocommit to False for transaction control
            connection.autocommit = False
            return connection
        except Exception as e:
            logger.error(f"Failed to connect to PostgreSQL database: {e}")
            raise
    
    def _read_dep_file(self, dep_path: str) -> List[str]:
        """
        Read script paths from an .DEP file.
        
        Args:
            dep_path: Path to the .DEP file
            
        Returns:
            List of script paths
        """
        script_paths = []
        try:
            with open(dep_path, 'r') as f:
                for line in f:
                    line = line.strip()
                    if line and not line.startswith('#'):  # Skip empty lines and comments
                        script_paths.append(line)
            return script_paths
        except Exception as e:
            logger.error(f"Error reading DEP file {dep_path}: {e}")
            raise
    
    def _execute_sql_script(self, conn, script_path: str) -> None:
        """
        Execute a PostgreSQL script.
        
        Args:
            conn: Database connection
            script_path: Path to the SQL script
        """
        try:
            with open(script_path, 'r') as f:
                sql_content = f.read()
                
            cursor = conn.cursor()
            
            # For PostgreSQL, we can execute the entire script at once
            # psycopg2 handles script with multiple statements properly
            try:
                logger.info(f"Executing SQL script: {script_path}")
                cursor.execute(sql_content)
                conn.commit()
                logger.info(f"Successfully executed script: {script_path}")
            except Exception as e:
                conn.rollback()
                logger.error(f"Error executing SQL script {script_path}: {e}")
                raise
            finally:
                cursor.close()
        except Exception as e:
            logger.error(f"Error processing SQL script {script_path}: {e}")
            raise
    
    def check_for_new_deps(self) -> List[str]:
        """
        Check for new .DEP files in the directory.
        
        Returns:
            List of paths to new .DEP files
        """
        new_deps = []
        try:
            if not os.path.exists(self.dep_dir):
                logger.warning(f"DEP directory does not exist: {self.dep_dir}")
                return new_deps
                
            for filename in os.listdir(self.dep_dir):
                if filename.lower().endswith('.dep'):
                    dep_path = os.path.join(self.dep_dir, filename)
                    if dep_path not in self.processed_deps:
                        new_deps.append(dep_path)
            return new_deps
        except Exception as e:
            logger.error(f"Error checking for new DEP files: {e}")
            raise
    
    def process_dep_file(self, dep_path: str) -> None:
        """
        Process a single .DEP file.
        
        Args:
            dep_path: Path to the .DEP file
        """
        logger.info(f"Processing DEP file: {dep_path}")
        
        # Establish database connection once per DEP file
        conn = None
        
        try:
            # Read script paths from the .DEP file
            script_paths = self._read_dep_file(dep_path)
            
            if not script_paths:
                logger.warning(f"No script paths found in {dep_path}")
                self._save_processed_dep(dep_path)
                return
            
            # Connect to the database
            conn = self._connect_to_db()
                
            # Execute each script in order
            for script_path in script_paths:
                # Handle relative paths
                if not os.path.isabs(script_path):
                    script_path = os.path.join(os.path.dirname(dep_path), script_path)
                
                if os.path.exists(script_path):
                    self._execute_sql_script(conn, script_path)
                else:
                    logger.error(f"Script file not found: {script_path}")
                    raise FileNotFoundError(f"Script file not found: {script_path}")
            
            # Mark DEP file as processed
            self._save_processed_dep(dep_path)
            logger.info(f"Successfully processed DEP file: {dep_path}")
            
        except Exception as e:
            logger.error(f"Failed to process DEP file {dep_path}: {e}")
            raise
        finally:
            # Close connection if opened
            if conn:
                conn.close()
    
    def run(self) -> int:
        """
        Run once and process any new .DEP files found.
        
        Returns:
            int: Number of .DEP files processed
        """
        logger.info(f"Starting PostgreSQL Script Runner. Processing .DEP files in {self.dep_dir}")
        logger.info(f"Database: PostgreSQL at {self.db_host}:{self.db_port}/{self.db_name}")
        
        try:
            # Check for new .DEP files
            new_deps = self.check_for_new_deps()
            
            if new_deps:
                logger.info(f"Found {len(new_deps)} new DEP file(s)")
                
                # Process each new .DEP file
                for dep_path in new_deps:
                    self.process_dep_file(dep_path)
                
                logger.info(f"Successfully processed {len(new_deps)} DEP file(s)")
                return len(new_deps)
            else:
                logger.info("No new DEP files found")
                return 0
                
        except Exception as e:
            logger.error(f"Unexpected error: {e}")
            raise

def main():
    """Parse command line arguments and start the script runner."""
    parser = argparse.ArgumentParser(description='PostgreSQL Auto Script Runner')
    parser.add_argument('--dep-dir', required=True, help='Directory to monitor for .DEP files')
    parser.add_argument('--db-host', required=True, help='PostgreSQL host')
    parser.add_argument('--db-name', required=True, help='PostgreSQL database name')
    parser.add_argument('--db-user', required=True, help='PostgreSQL username')
    parser.add_argument('--db-password', help='PostgreSQL password')
    parser.add_argument('--db-port', type=int, default=5432, help='PostgreSQL port (default: 5432)')
    
    args = parser.parse_args()
    
    try:
        # Create and run the PostgreSQL Script Runner
        runner = PostgresScriptRunner(
            dep_dir=args.dep_dir,
            db_host=args.db_host,
            db_name=args.db_name,
            db_user=args.db_user,
            db_password=args.db_password,
            db_port=args.db_port
        )
        
        num_processed = runner.run()
        sys.exit(0 if num_processed >= 0 else 1)
    except Exception as e:
        logger.error(f"Fatal error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()
    
    def run(self) -> int:
        """
        Run once and process any new .DEP files found.
        
        Returns:
            int: Number of .DEP files processed
        """
        logger.info(f"Starting SQL Auto Script Runner. Processing .DEP files in {self.dep_dir}")
        logger.info(f"Database: {self.db_type} at {self.db_host}:{self.db_port}/{self.db_name}")
        
        try:
            # Check for new .DEP files
            new_deps = self.check_for_new_deps()
            
            if new_deps:
                logger.info(f"Found {len(new_deps)} new DEP file(s)")
                
                # Process each new .DEP file
                for dep_path in new_deps:
                    self.process_dep_file(dep_path)
                
                logger.info(f"Successfully processed {len(new_deps)} DEP file(s)")
                return len(new_deps)
            else:
                logger.info("No new DEP files found")
                return 0
                
        except Exception as e:
            logger.error(f"Unexpected error: {e}")
            raise

def main():
    """Parse command line arguments and start the script runner."""
    parser = argparse.ArgumentParser(description='SQL Auto Script Runner')
    parser.add_argument('--dep-dir', required=True, help='Directory to monitor for .DEP files')
    parser.add_argument('--db-type', required=True, choices=['mysql', 'postgres'], 
                      help='Database type (mysql or postgres)')
    parser.add_argument('--db-host', required=True, help='Database host')
    parser.add_argument('--db-name', required=True, help='Database name')
    parser.add_argument('--db-user', required=True, help='Database username')
    parser.add_argument('--db-password', help='Database password')
    parser.add_argument('--db-port', type=int, help='Database port (default: 3306 for MySQL, 5432 for PostgreSQL)')
    
    args = parser.parse_args()
    
    try:
        # Create and run the SQL Auto Script Runner
        runner = SQLAutoScriptRunner(
            dep_dir=args.dep_dir,
            db_type=args.db_type,
            db_host=args.db_host,
            db_name=args.db_name,
            db_user=args.db_user,
            db_password=args.db_password,
            db_port=args.db_port
        )
        
        num_processed = runner.run()
        sys.exit(0 if num_processed >= 0 else 1)
    except Exception as e:
        logger.error(f"Fatal error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    main()